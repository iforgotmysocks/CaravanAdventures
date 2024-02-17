using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;


namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        private bool wasEnabled = false;
        public float BaseDelayNextShrineReveal => Helper.Debug() ? 1800f : 60000f * 3f;
        private float shrineRevealCounter = -1f;
        private int ticks = -1;
        private int countShrinesCompleted = 0;
        private int shrineMaximum = 5;
        public bool wasShrineAmbushNoLuck = false;

        private readonly IntRange timeoutDaysRange = new IntRange(10, 12);

        public IntRange ShrineDistance => Helper.Debug()
            ? new IntRange(2, 4)
            : wasShrineAmbushNoLuck
                ? new IntRange(ModSettings.shrineDistance.min / 5, ModSettings.shrineDistance.max / 5)
                : ModSettings.shrineDistance;

        private int shrineTileUnsuccessfulCounter = 0;
        private List<AbilityDef> unlockedSpells = new List<AbilityDef>();
        private bool ranDebugActionsOnceAtStartUp;
        private IEnumerable<ThingDef> bossDefs;

        public Dictionary<PawnKindDef, int> mechBossKillCounters = new Dictionary<PawnKindDef, int>();
        public Dictionary<string, bool> debugFlags = new Dictionary<string, bool>()
        {
            { "ShowDebugInfo", true },
            { "DebugAllAbilities", false },

            { "FriendlyCaravanDone", false },
            { "VillageDone", false },
            { "StoryStartDone", false },
            { "ForwardToLastShrine", false },
            { "ShrinesDone", false },
        };
        public Dictionary<string, bool> storyFlags;
        private List<string> flagsToAdd = new List<string>
        {
            "ShowedSetupMenu",

            "TradeCaravan_InitCountDownStarted",
            "TradeCaravan_Arrived",
            "TradeCaravan_DialogFinished",

            "IntroVillage_InitCountDownStarted",
            "IntroVillage_Created",
            "IntroVillage_Entered",
            "IntroVillage_TalkedToFriend",
            "IntroVillage_FriendAlreadyDeadOrLeft",
            "IntroVillage_MechsArrived",
            "IntroVillage_ReinforcementsArrived",
            "IntroVillage_PlayerWon",
            "IntroVillage_Finished",

            "Start_InitialTreeWhisper",
            "Start_TreeWhisperQuestStarted",
            "Start_InitialTreeAddTalkOption",
            "Start_CanReceiveGift",
            "Start_TreeTalkedTo",
            "Start_ReceivedGift",

            "SacrilegHuntersBetrayal",

            "Judgment_ApocalypseStarted",
            "Judgment_PortalRevealed",
            "Judgment_Created",
            "Judgment_StoryOverDialog",
            "Judgment_Completed",
        };

        public QuestCont questCont;

        // not saved for now
        private bool doEarthQuake = false;
        private Sustainer earthquakeSustainer;

        // todo remove when friendly mech faction is removed
        private int mechFactionRemovalTicks = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storyFlags, "storyFlags", LookMode.Value);
            Scribe_Collections.Look(ref unlockedSpells, "unlockedSpells", LookMode.Def);
            Scribe_Collections.Look(ref mechBossKillCounters, "mechBossKillCounters", LookMode.Def);
            Scribe_Values.Look(ref ticks, "ticks", -1);
            Scribe_Values.Look(ref shrineRevealCounter, "shrineRevealCounter", -1);
            Scribe_Values.Look(ref countShrinesCompleted, "countShrinesCompleted", 0);
            Scribe_Deep.Look(ref questCont, "questCont");
            Scribe_Values.Look(ref shrineTileUnsuccessfulCounter, "shrineTileUnsuccessfulCounter", 0);
            Scribe_Values.Look(ref wasEnabled, "wasEnabled", false);
            Scribe_Values.Look(ref wasShrineAmbushNoLuck, "wasShrineAmbushNoLuck", false);
        }

        public StoryWC(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CompCache.StoryWC = null;
            Patches.Compatibility.SoS2Patch.Reset();

            InitializeStoryFlags();
            InitializeQuestCont();

            if (debugFlags["StoryStartDone"])
                foreach (var flag in storyFlags.Where(x => x.Key.StartsWith("Start_")).ToList())
                    storyFlags[flag.Key] = true;

            if (debugFlags["ForwardToLastShrine"])
            {
                countShrinesCompleted = 4;
                foreach (var flag in storyFlags.Where(x => x.Key.StartsWith("Shrine") && !x.Key.StartsWith("Shrine5_")).ToList())
                    storyFlags[flag.Key] = true;
            }

            if (ModSettings.debugMessages) storyFlags.ToList().ForEach(flag => Log.Message($"{flag.Key} {flag.Value}"));
        }

        private void InitializeStoryFlags()
        {
            if (storyFlags == null) storyFlags = new Dictionary<string, bool>();
            for (int i = 1; i < shrineMaximum + 2; i++)
            {
                flagsToAdd.Add("Shrine" + i + "_InitCountDownStarted");
                flagsToAdd.Add("Shrine" + i + "_Created");
                flagsToAdd.Add("Shrine" + i + "_Completed");
            }

            foreach (var flag in flagsToAdd)
            {
                if (!storyFlags.ContainsKey(flag)) storyFlags[flag] = false;
            }
        }

        private void InitializeQuestCont()
        {
            questCont = questCont ?? new QuestCont();
            questCont.EnsureFieldsInitialized();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            RunDebugActionsOnceAtStartUp();

            if (!RoyaltyActiveCheck()) return;

            if (ticks == -1) StoryUtility.EnsureSacrilegHunters(FactionRelationKind.Neutral);

            if (ticks == 120 && !storyFlags["ShowedSetupMenu"] && !ModSettings.disableSetupWindow)
            {
                Find.WindowStack.Add(new Dialogs.NewStorySettings());
                storyFlags["ShowedSetupMenu"] = true;
            }

            // todo temporary - remove once friendly mech faction is removed
            if (mechFactionRemovalTicks >= 22222)
            {
                mechFactionRemovalTicks = 0;
                Helper.RunSafely(() => StoryUtility.RemoveFaction(), false, "", true);
            }

            if (doEarthQuake && ticks > 0 && ticks <= 1200)
            {
                if (earthquakeSustainer == null) earthquakeSustainer = SoundDef.Named("CAEarthquake").TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
                earthquakeSustainer.Maintain();
                Find.CameraDriver.shaker.DoShake(10);
                if (ticks == 1200)
                {
                    earthquakeSustainer.End();
                    doEarthQuake = false;
                }
            }

            if (ticks > 1200)
            {
                ticks = 0;

                if (ModSettings.delayStoryDays != 0 && ModSettings.delayStoryDays * 60000 > Find.TickManager.TicksGame && !ModSettings.debug)
                {
                    //DLog.Message($"Delaying story. Current ticks: {Find.TickManager.TicksGame} / {ModSettings.delayStoryDays * 60000}");
                    return;
                }
                StoryUtility.GenerateStoryContact();
                if (CheckCanStartFriendlyCaravanCounter() && !debugFlags["FriendlyCaravanDone"])
                {
                    questCont.FriendlyCaravan.friendlyCaravanCounter = questCont.FriendlyCaravan.BaseDelayFriendlyCaravan;
                    DLog.Message("friendlycaravan counter running " + questCont.FriendlyCaravan.friendlyCaravanCounter);
                    SetSF("TradeCaravan_InitCountDownStarted");
                }

                if (CheckCanStartVillageGenerationCounter() && !debugFlags["VillageDone"])
                {
                    questCont.Village.villageGenerationCounter = questCont.Village.BaseDelayVillageGeneration;
                    DLog.Message("village gen counter running " + questCont.Village.villageGenerationCounter);
                    SetSF("IntroVillage_InitCountDownStarted");
                }

                if (CheckShouldWatchOverVillage())
                {
                    DLog.Message($"Checking for settlement availability. Counter: {questCont.Village.villageGenerationCounter}");
                    Helper.RunSafely(questCont.Village.TryCheckVillageAndEnsure);
                }

                if (CheckCanStartCountDownOnNewShrine() && !debugFlags["ShrinesDone"])
                {
                    shrineRevealCounter = BaseDelayNextShrineReveal * (countShrinesCompleted + 1f) * 0.5f;
                    DLog.Message("Shrine counter running " + shrineRevealCounter);
                    SetShrineSF("InitCountDownStarted");
                }

                if (CanDoApocalypse())
                {
                    doEarthQuake = true;
                    SetSF("Judgment_ApocalypseStarted");
                    questCont.LastJudgment.StartApocalypse(-0.1f);
                }
            }
            if (questCont.FriendlyCaravan.friendlyCaravanCounter == 0) Helper.RunSavelyWithDelay(() =>
                    CompCache.StoryWC.questCont.FriendlyCaravan.TryCreateFriendlyCaravan(
                        ref questCont.FriendlyCaravan.friendlyCaravanCounter),
                    ref questCont.FriendlyCaravan.friendlyCaravanCounter, 60000);

            if (questCont.Village.villageGenerationCounter == 0) Helper.RunSavelyWithDelay(() =>
                StoryUtility.GenerateFriendlyVillage(
                    ref questCont.Village.villageGenerationCounter, questCont.Village.RespawnVillage),
                ref questCont.Village.villageGenerationCounter);

            if (shrineRevealCounter == 0) Helper.RunSavelyWithDelay(() =>
                TryCreateNewShrine(
                    ref shrineRevealCounter),
                ref shrineRevealCounter);

            ticks++;
            questCont.FriendlyCaravan.friendlyCaravanCounter--;
            questCont.Village.villageGenerationCounter--;
            shrineRevealCounter--;

            mechFactionRemovalTicks++;
        }

        private bool RoyaltyActiveCheck()
        {
            if (!ModsConfig.RoyaltyActive || !ModSettings.storyEnabled)
            {
                if (wasEnabled) StoryUtility.RemoveStoryComponentsNoRoyalty();
                wasEnabled = false;
                return false;
            }
            if (!InitPatches.storyPatchesLoaded) return false;
            wasEnabled = true;
            return true;
        }

        private void RunDebugActionsOnceAtStartUp()
        {
            if (ranDebugActionsOnceAtStartUp) return;

            DLog.Message($"Applying debug actions once");
            // todo added cleanup of faction settlement in 1.2.4 to be able to remove CAFriendlyMechanoid faction in a couple patches
            Helper.RunSafely(() => StoryUtility.RemoveFaction(), false, "", true);

            //CompatibilityPatches.TryRegionStuff();
            if (Helper.ExpRM) StoryUtility.EnsureEvilHostileFactionForExpansion(true);

            // todo disable
            Helper.PrintWorldPawns();

            ranDebugActionsOnceAtStartUp = true;
        }

        public IEnumerable<ThingDef> BossDefs() => bossDefs ?? (bossDefs = StoryUtility.ReadBossDefNames());

        public void IncreaseShrineCompleteCounter() => countShrinesCompleted++;

        private void TryCreateNewShrine(ref float shrineRevealCounter)
        {
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                DLog.Message($"Skipping shrine revealation, Sac hunters are hostile.");
                shrineRevealCounter = 20000;
                return;
            }

            var tile = GetShrineLocation();
            if (tile == -1)
            {
                DLog.Message($"Skipping shrine revealation, couldn't find valid location.");
                shrineRevealCounter = 1000;
                return;
            }

            shrineTileUnsuccessfulCounter = 0;
            var ancientMasterShrineWO = (AncientMasterShrineWO)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.CAAncientMasterShrineWO);
            ancientMasterShrineWO.Tile = tile;
            Find.WorldObjects.Add(ancientMasterShrineWO);

            StoryUtility.GenerateStoryContact();
            Find.LetterStack.ReceiveLetter("Story_Shrine1_NewShrineDetectedLetterLabel".Translate(), "Story_Shrine1_NewShrineDetectedLetterMessage".Translate(CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored), LetterDefOf.PositiveEvent, ancientMasterShrineWO);
            Quests.QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_FindAncientShrine, true, "Story_Shrine1_QuestName", null, "Story_Shrine1_QuestDesc", new object[] { CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored });
            Quests.QuestUtility.UpdateQuestLocation(StoryQuestDefOf.CA_FindAncientShrine, ancientMasterShrineWO);
            SetShrineSF("Created");
        }

        private int GetShrineLocation()
        {
            int tile = -1;
            var newRange = new IntRange(ShrineDistance.min / (shrineTileUnsuccessfulCounter + 1), ShrineDistance.max);
            if (CompatibilityDefOf.CACompatDef.excludedBiomeDefNamesForStoryShrineGeneration.Any())
            {
                DLog.Message($"Trying to skip biome defs.");
                var startTile = -1;
                TileFinder.TryFindRandomPlayerTile(out startTile, true);
                if (startTile == -1) startTile = Find.World.worldObjects?.Caravans?.FirstOrDefault(x => x?.IsPlayerControlled == true)?.Tile ?? -1;
                if (!TileFinder.TryFindPassableTileWithTraversalDistance(startTile, newRange.min, newRange.max, out tile, (int x) =>
                    !CompatibilityDefOf.CACompatDef.excludedBiomeDefNamesForStoryShrineGeneration.Contains(Find.World.grid[x].biome?.defName)))
                {
                    shrineTileUnsuccessfulCounter++;
                    DLog.Message("Couldn't find tile to create a new shrine");
                    shrineRevealCounter = 180f;
                    return -1;
                }
            }
            else
            {
                if (!TileFinder.TryFindNewSiteTile(out tile, newRange.min, newRange.max))
                {
                    shrineTileUnsuccessfulCounter++;
                    DLog.Message("Couldn't find tile to create a new shrine");
                    shrineRevealCounter = 180f;
                    return -1;
                }
            }

            return tile;
        }

        public void SetSF(string key) => storyFlags[key] = true;
        public void SetShrineSF(string postFix) => storyFlags[storyFlags.Keys.FirstOrDefault(x => x.StartsWith(BuildCurrentShrinePrefix() + postFix))] = true;
        public void ResetCurrentShrineFlags() => storyFlags.Keys.Where(x => x.StartsWith(BuildCurrentShrinePrefix())).ToList().ForEach(key => storyFlags[key] = false);
        public void SetSFsStartingWith(string start, bool value = false) => storyFlags.Keys.Where(x => x.StartsWith(start)).ToList().ForEach(key => storyFlags[key] = value);
        public string BuildCurrentShrinePrefix(bool ignoreLimit = false) => ignoreLimit ? "Shrine" + countShrinesCompleted + 1 : "Shrine" + (countShrinesCompleted < shrineMaximum + 1 ? countShrinesCompleted + 1 : shrineMaximum + 1) + "_";
        public string BuildMaxShrinePrefix() => "Shrine" + shrineMaximum + "_";
        public int GetCurrentShrineCounter(bool ignoreLimit = false) => ignoreLimit ? countShrinesCompleted + 1 : countShrinesCompleted < shrineMaximum + 1 ? countShrinesCompleted + 1 : shrineMaximum + 1;
        public int GetShrineMaxiumum => shrineMaximum;

        public List<AbilityDef> GetUnlockedSpells() => unlockedSpells;

        private bool CheckCanStartCountDownOnNewShrine() =>
            storyFlags["Start_ReceivedGift"]
            && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            ||
            storyFlags["Start_ReceivedGift"]
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == true)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            && (countShrinesCompleted < shrineMaximum || countShrinesCompleted >= shrineMaximum && ModSettings.issueFurtherShrineLocationsAfterStoryEnd)
            && Find.World?.worldObjects?.AllWorldObjects?.OfType<AncientMasterShrineMP>()?.Count() == 0;

        private bool CheckCanStartFriendlyCaravanCounter() => !storyFlags["TradeCaravan_InitCountDownStarted"];
        private bool CheckCanStartVillageGenerationCounter() => storyFlags["TradeCaravan_DialogFinished"] && !storyFlags["IntroVillage_InitCountDownStarted"];
        private bool CanDoApocalypse() => !storyFlags["Judgment_ApocalypseStarted"]
            && storyFlags["Shrine1_Completed"]
            && !storyFlags[BuildMaxShrinePrefix() + "Completed"]
            && ModSettings.apocalypseEnabled;

        private bool CheckShouldWatchOverVillage() => storyFlags["IntroVillage_Created"] && !storyFlags["IntroVillage_Entered"];

        public void ResetStoryVars()
        {
            storyFlags.Keys.ToList().ForEach(key => storyFlags[key] = false);
            mechBossKillCounters.Clear();

            shrineRevealCounter = -1;
            ticks = -1;
            countShrinesCompleted = 0;
            unlockedSpells = new List<AbilityDef>();
        }

        public void SetStoryCompleteVars()
        {
            storyFlags.Keys.Where(x => x != "SacrilegHuntersBetrayal").ToList().ForEach(key => storyFlags[key] = true);
            unlockedSpells = new List<AbilityDef>();
            foreach (var spell in DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CAAncient"))) unlockedSpells.Add(spell);

            mechBossKillCounters.Clear();
            shrineRevealCounter = -1;
            ticks = -1;
            countShrinesCompleted = 5;

            CompCache.BountyWC.BountyServiceAvailable = true;
            CompCache.BountyWC.BountyNotificationCounter = -1;
        }


    }
}

