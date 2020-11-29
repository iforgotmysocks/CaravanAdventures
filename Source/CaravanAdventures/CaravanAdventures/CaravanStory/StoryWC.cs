using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

// high prio:
// - change rewards
// --> maybe remove additional reward generation for shrines for a bounty system
// --> integrate point system with buyable items into the comm station dialog

// - bounty system
// --> finish items
// --> finish relation haggling

// - story: 
// --> make sure that sac hunters won't die of starvation and or temperature
// --> if they flee, what then? fail the quest?
// --> what to do when they get hostile
// --> make sure player cant enter village when hostile

// - unlag
// --> remove additional ancient caskets from small shrines on master shrine map --> lets hold this one and better balance the amount correctly
// --> maybe decrease small shrines and make them not depend on spawning on a rock

// - better map generation control for small shrines
// --> create own GenStepDef for small shrines (currently using vanilla) and remove the CanScatterAt natural rock limitation
// --> rebalance min/max accordingly

// - kill all walls on a master shrine when the boss dies -> solves the letter issue
// - second shrine was removed before the boss died and the ability dialog came up
// - fix just melee mech raids, by adding a new pawngroupmaker to the faction def that isn't called combat and use that
// - crystal scythe appeared in normal ancient shrine -> may have been fixed by recent point changes
// - add and rework dialogs from shrines onwards
// - add system that allows inclusion or exclusion for other races to relation
// - suddenly yellow save error for the sacrileg hunter faction appeared and the story char was generated anew -> fix!!
// - village still throws an error sometimes, when story char is being spawned on a unwalkable cell: 
//Couldn't find a cell to spawn pawn
//Verse.Log:Error(String, Boolean)
//CaravanAdventures.CaravanStory.<> c__DisplayClass8_0:< Notify_CaravanArrived > b__1()
//Verse.LongEventHandler:UpdateCurrentSynchronousEvent(Boolean &)
//Verse.LongEventHandler:LongEventsUpdate(Boolean &)
//Verse.Root:Update()
//Verse.Root_Play:Update()

// med prio:
// - add check to master shrine location selection, that keeps it away from a player settlement
// - seperate bounty system from story
// --> allow to set another faction in settings that offers the bounties
// --> implement rewards with and without royalty

// low prio:
// - make configurable if sacrileg hunters are hostile towards the empire
// - collect player responses and use them to determine the support strength for troups at shrines
// - balance village hunter strenght depending on player character wealth
// - export shrine stuff to seperate questCont

// etc: 
// {PAWN_pronoun} in text possible? -> GenderUtility.GetPronoun()

namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        private readonly float baseDelayNextShrineReveal = Helper.Debug() ? 1800f : 60000f * 3f;
        private float shrineRevealCounter = -1f;
        private int ticks = -1;
        private int countShrinesCompleted = 0;
        private int shrineMaximum = 5;

        private readonly IntRange timeoutDaysRange = new IntRange(10, 12);
        // todo move baseDelayNextShrineReveal and shrineDistance to settings?
        private readonly IntRange shrineDistance = Helper.Debug() ? new IntRange(2, 4) : new IntRange(300, 500);
        private int shrineTileUnsuccessfulCounter = 0;
        private List<AbilityDef> unlockedSpells = new List<AbilityDef>();
        private int bossMissedCounter = 0;
        private bool ranDebugActionsOnceAtStartUp;
        private float bountyPoints = 0;

        public Dictionary<string, int> mechBossKillCounters = new Dictionary<string, int>();
        public Dictionary<string, bool> debugFlags = new Dictionary<string, bool>()
        {
            { "ShowDebugInfo", true },
            { "DebugResetVillagesAndShrines", false },
            { "DebugAllAbilities", false },

            { "FriendlyCaravanDone", false },
            { "VillageDone", false },
            { "StoryStartDone", false },
            { "ForwardToLastShrine", false },
            { "ShrinesDone", false },
            { "ResetApocalypse", false },
        };
        public Dictionary<string, bool> storyFlags;
        private List<string> flagsToAdd = new List<string>
        {
            "TradeCaravan_InitCountDownStarted",
            "TradeCaravan_Arrived",
            "TradeCaravan_DialogFinished",

            "IntroVillage_InitCountDownStarted",
            "IntroVillage_Created",
            "IntroVillage_Entered",
            "IntroVillage_TalkedToFriend",
            "IntroVillage_MechsArrived",
            "IntroVillage_ReinforcementsArrived",
            "IntroVillage_PlayerWon",
            "IntroVillage_Finished",

            "Start_InitialTreeWhisper",
            "Start_InitialTreeAddTalkOption",
            "Start_CanReceiveGift",
            "Start_ReceivedGift",

            "SacrilegHuntersBetrayal",

            "Judgment_ApocalypseStarted",
            "Judgment_Created",
            "Judgment_StoryOverDialog",
            "Judgment_Completed",
        };

        public QuestCont questCont;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storyFlags, "storyFlags", LookMode.Value);
            Scribe_Collections.Look(ref unlockedSpells, "unlockedSpells", LookMode.Def);
            Scribe_Collections.Look(ref mechBossKillCounters, "mechBossKillCounters", LookMode.Value);
            Scribe_Values.Look(ref ticks, "ticks", -1);
            Scribe_Values.Look(ref shrineRevealCounter, "shrineRevealCounter", -1);
            Scribe_Values.Look(ref countShrinesCompleted, "countShrinesCompleted", 0);
            Scribe_Values.Look(ref bossMissedCounter, "bossMissedCounter", 0);
            Scribe_Deep.Look(ref questCont, "questCont");
            Scribe_Values.Look(ref shrineTileUnsuccessfulCounter, "shrineTileUnsuccessfulCounter", 0);
            Scribe_Values.Look(ref bountyPoints, "bountyPoints", 0f);
        }

        public StoryWC(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CompCache.StoryWC = null;
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

            if (debugFlags["ShowDebugInfo"]) storyFlags.ToList().ForEach(flag => Log.Message($"{flag.Key} {flag.Value}"));
        }

        private void InitializeStoryFlags()
        {
            if (storyFlags == null) storyFlags = new Dictionary<string, bool>();
            for (int i = 1; i < shrineMaximum + 1; i++)
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
            if (ticks == -1) StoryUtility.EnsureSacrilegHunters();

            if (ticks > 1200)
            {
                StoryUtility.GenerateStoryContact();
                if (CheckCanStartFriendlyCaravanCounter() && !debugFlags["FriendlyCaravanDone"])
                {
                    questCont.FriendlyCaravan.friendlyCaravanCounter = questCont.FriendlyCaravan.baseDelayFriendlyCaravan;
                    if (Dg) Log.Message("friendlycaravan counter running " + questCont.FriendlyCaravan.friendlyCaravanCounter);
                    SetSF("TradeCaravan_InitCountDownStarted");
                }

                if (CheckCanStartVillageGenerationCounter() && !debugFlags["VillageDone"])
                {
                    questCont.Village.villageGenerationCounter = questCont.Village.baseDelayVillageGeneration;
                    if (Dg) Log.Message("village gen counter running " + questCont.Village.villageGenerationCounter);
                    SetSF("IntroVillage_InitCountDownStarted");
                }

                if (CheckCanStartCountDownOnNewShrine() && !debugFlags["ShrinesDone"])
                {
                    shrineRevealCounter = baseDelayNextShrineReveal * (countShrinesCompleted + 1f) * 0.5f;
                    if (Dg) Log.Message("Shrine counter running " + shrineRevealCounter);
                    SetShrineSF("InitCountDownStarted");
                }

                // todo add mod setting and ability to disable
                if (CanDoApocalypse()) questCont.LastJudgment.StartApocalypse(-10f);

                ticks = 0;
            }
            if (questCont.FriendlyCaravan.friendlyCaravanCounter == 0) CompCache.StoryWC.questCont.FriendlyCaravan.TryCreateFriendlyCaravan(ref questCont.FriendlyCaravan.friendlyCaravanCounter);
            if (questCont.Village.villageGenerationCounter == 0) StoryUtility.GenerateFriendlyVillage(ref questCont.Village.villageGenerationCounter);
            if (shrineRevealCounter == 0) TryCreateNewShrine(ref shrineRevealCounter);
            
            ticks++;
            questCont.FriendlyCaravan.friendlyCaravanCounter--;
            questCont.Village.villageGenerationCounter--;
            shrineRevealCounter--;
        }
        
        private void RunDebugActionsOnceAtStartUp()
        {
            if (ranDebugActionsOnceAtStartUp) return;

            if (debugFlags["DebugResetVillagesAndShrines"])
            {
                StoryUtility.RemoveExistingQuestFriendlyVillages();
                StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineMP);
                StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineWO);
                ResetCurrentShrineFlags();
                ResetSFsStartingWith("IntroVillage");
            }

            if (debugFlags["ResetApocalypse"]) questCont.LastJudgment.ResetApocalypse(-10f);

            ranDebugActionsOnceAtStartUp = true;
        }

        public string[] GetBossDefNames() => new string[] { "CACristalScythe", "CABossMechDevourer" };

        public void IncreaseShrineCompleteCounter() => countShrinesCompleted++;

        private void TryCreateNewShrine(ref float shrineRevealCounter)
        {
            // todo Create own find method that keeps the same distance from bases and caravans
            // -> after an unsuccesfull attempt, select tile that supports it for sure.
            var newRange = new IntRange(shrineDistance.min / (shrineTileUnsuccessfulCounter + 1), shrineDistance.max / (shrineTileUnsuccessfulCounter + 1));
            if (!TileFinder.TryFindNewSiteTile(out var tile, newRange.min, newRange.max))
            {
                shrineTileUnsuccessfulCounter++;
                Log.Message("Couldn't find tile to create a new shrine");
                shrineRevealCounter = 60f;
                return;
            }
            shrineTileUnsuccessfulCounter = 0;
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                Log.Message($"Skipping shrine revealation, Sac hunters are hostile.");
                shrineRevealCounter = 20000;
                return;
            }
            var ancientMasterShrineWO = (AncientMasterShrineWO)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.CAAncientMasterShrineWO);
            ancientMasterShrineWO.Tile = tile;
            //ancientMasterShrineWO.GetComponent<TimeoutComp>().StartTimeout(timeoutDaysRange.RandomInRange * 60000);
            Find.WorldObjects.Add(ancientMasterShrineWO);

            // todo turn into quest
            // todo figure out how to append location links into quest
            Find.LetterStack.ReceiveLetter("Story_Shrine1_NewShrineDetectedLetterLabel".Translate(), "Story_Shrine1_NewShrineDetectedLetterMessage".Translate(CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored), LetterDefOf.PositiveEvent, ancientMasterShrineWO);
            Quests.QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_FindAncientShrine, true, "Story_Shrine1_QuestName", null, "Story_Shrine1_QuestDesc", new object[] { CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored });
            Quests.QuestUtility.UpdateQuestLocation(StoryQuestDefOf.CA_FindAncientShrine, ancientMasterShrineWO);
            SetShrineSF("Created");
        }

        // SF helper methods used to be static, if the CompCache doesn't work out, turn access to the SFs static again
        public bool Dg => debugFlags["ShowDebugInfo"];
        public void SetSF(string key) => storyFlags[key] = true;
        public void SetShrineSF(string postFix) => storyFlags[storyFlags.Keys.FirstOrDefault(x => x.StartsWith(BuildCurrentShrinePrefix() + postFix))] = true;
        public void ResetCurrentShrineFlags() => storyFlags.Keys.Where(x => x.StartsWith(BuildCurrentShrinePrefix())).ToList().ForEach(key => storyFlags[key] = false);
        public void ResetSFsStartingWith(string start) => storyFlags.Keys.Where(x => x.StartsWith(start)).ToList().ForEach(key => storyFlags[key] = false);
        public string BuildCurrentShrinePrefix() => "Shrine" + (countShrinesCompleted < shrineMaximum ? countShrinesCompleted + 1 : shrineMaximum) + "_";
        public string BuildMaxShrinePrefix() => "Shrine" + shrineMaximum + "_";
        public int GetCurrentShrineCounter => countShrinesCompleted < shrineMaximum ? countShrinesCompleted + 1 : shrineMaximum;
        public int GetShrineMaxiumum => shrineMaximum;
        public float BountyPoints { get => bountyPoints; set => bountyPoints = value; }

        public List<AbilityDef> GetUnlockedSpells() => unlockedSpells;
        
        private bool CheckCanStartCountDownOnNewShrine() =>
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            || 
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == true)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            && countShrinesCompleted < shrineMaximum;

        // todo - incomplete
        private bool CheckCanStartFriendlyCaravanCounter() => !storyFlags["TradeCaravan_InitCountDownStarted"];
        private bool CheckCanStartVillageGenerationCounter() => storyFlags["TradeCaravan_DialogFinished"] && !storyFlags["IntroVillage_InitCountDownStarted"];
        private bool CanDoApocalypse() => !storyFlags["Judgment_ApocalypseStarted"] 
            && storyFlags["Shrine1_Completed"] 
            && !storyFlags[BuildMaxShrinePrefix() + "Completed"] 
            && ModSettings.Get().apocalypseEnabled;

        public void ResetStoryVars()
        {
            storyFlags.Keys.ToList().ForEach(key => storyFlags[key] = false);
            mechBossKillCounters.Keys.ToList().ForEach(key => mechBossKillCounters[key] = 0);

            shrineRevealCounter = -1;
            ticks = -1;
            countShrinesCompleted = 0;
            bossMissedCounter = 0;
            unlockedSpells = new List<AbilityDef>();
        }


    }
}

