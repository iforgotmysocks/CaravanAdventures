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

// add mod option to regain the gift at the anima tree instead of passing it on to someone else
// todo wake up shrine mechs if other factions pawns come close or best when wall breaks

// add a weather condition that freezes or heats the world until the main boss is defeated. -> must be disableable in the options

// create mech bosses after horsemen traits
// add quote to each horsemen type when entering the shrine
// credits to horsemen idea to Shakesthespeare

// todo -> collect player responses and use them to determine the support strength for troups at shrines
// todo -> balance village hunter strenght depending on player character wealth

// todo -> export shrine stuff to seperate questCont

namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        private readonly float baseDelayNextShrineReveal = Helper.Debug() ? 1800f : 60000f * 3f;
        private float shrineRevealCounter = -1f;
        private int ticks = -1;
        private float countShrinesCompleted = 0f;
        private readonly IntRange timeoutDaysRange = new IntRange(10, 12);
        private readonly IntRange shrineDistance = Helper.Debug() ? new IntRange(2, 4) : new IntRange(40, 60);
        private List<AbilityDef> unlockedSpells = new List<AbilityDef>();
        private int bossMissedCounter = 0;
        private bool ranDebugActionsOnceAtStartUp;

        public Dictionary<string, int> mechBossKillCounters = new Dictionary<string, int>();
        public Dictionary<string, bool> debugFlags = new Dictionary<string, bool>()
        {
            { "ShowDebugInfo", true },
            { "StoryStartDone", false },
            { "FriendlyCaravanDisabled", false },
            { "ShrinesDisabled", false },
            { "DebugAllAbilities", false },
            { "VillageDisabled", false },
            { "VillageFinished", false },
            { "DebugResetVillagesAndShrines", false },
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

            if (debugFlags["ShowDebugInfo"]) storyFlags.ToList().ForEach(flag => Log.Message($"{flag.Key} {flag.Value}"));
        }

        private void InitializeStoryFlags()
        {
            if (storyFlags == null) storyFlags = new Dictionary<string, bool>();
            for (int i = 1; i < 6; i++)
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
                if (CheckCanStartFriendlyCaravanCounter() && !debugFlags["FriendlyCaravanDisabled"])
                {
                    questCont.FriendlyCaravan.friendlyCaravanCounter = questCont.FriendlyCaravan.baseDelayFriendlyCaravan;
                    if (Dg) Log.Message("friendlycaravan counter running " + questCont.FriendlyCaravan.friendlyCaravanCounter);
                    SetSF("TradeCaravan_InitCountDownStarted");
                }

                if (CheckCanStartVillageGenerationCounter() && !debugFlags["VillageDisabled"])
                {
                    questCont.Village.villageGenerationCounter = questCont.Village.baseDelayVillageGeneration;
                    if (Dg) Log.Message("village gen counter running " + questCont.Village.villageGenerationCounter);
                    SetSF("IntroVillage_InitCountDownStarted");
                }

                if (CheckCanStartCountDownOnNewShrine() && !debugFlags["ShrinesDisabled"])
                {
                    shrineRevealCounter = baseDelayNextShrineReveal * (countShrinesCompleted + 1f) * 0.5f;
                    if (Dg) Log.Message("Shrine counter running " + shrineRevealCounter);
                    SetShrineSF("InitCountDownStarted");
                }

                //questCont.LastJudgment.StartApocalypse(-50, -10);
                //if (CheckCanStartApocalypse()) questCont.LastJudgment.StartApocalypse(-50, -10);

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

            ranDebugActionsOnceAtStartUp = true;
        }

        public string[] GetBossDefNames() => new string[] { "CACristalScythe", "CABossMechDevourer" };

        public void IncreaseShrineCompleteCounter() => countShrinesCompleted++;

        private void TryCreateNewShrine(ref float shrineRevealCounter)
        {
            // todo Create own find method that keeps the same distance from bases and caravans
            // -> after an unsuccesfull attempt, select tile that supports it for sure.
            if (!TileFinder.TryFindNewSiteTile(out var tile, shrineDistance.min, shrineDistance.max))
            {
                Log.Message("Couldn't find tile to create a new shrine");
                shrineRevealCounter = 60f;
                return;
            }
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
            Find.LetterStack.ReceiveLetter("Story_Shrine1_NewShrineDetectedLetterLabel".Translate(), "Story_Shrine1_NewShrineDetectedLetterMessage".Translate(), LetterDefOf.PositiveEvent, ancientMasterShrineWO);
            SetShrineSF("Created");
        }

        // SF helper methods used to be static, if the CompCache doesn't work out, turn access to the SFs static again
        public bool Dg => debugFlags["ShowDebugInfo"];
        public void SetSF(string key) => storyFlags[key] = true;
        public void SetShrineSF(string postFix) => storyFlags[storyFlags.Keys.FirstOrDefault(x => x.StartsWith(BuildCurrentShrinePrefix() + postFix))] = true;
        public void ResetCurrentShrineFlags() => storyFlags.Keys.Where(x => x.StartsWith(BuildCurrentShrinePrefix())).ToList().ForEach(key => storyFlags[key] = false);
        public void ResetSFsStartingWith(string start) => storyFlags.Keys.Where(x => x.StartsWith(start)).ToList().ForEach(key => storyFlags[key] = false);
        public string BuildCurrentShrinePrefix() => "Shrine" + (countShrinesCompleted + 1) + "_";
        public List<AbilityDef> GetUnlockedSpells() => unlockedSpells;
        
        // todo check if we even need TradeCaravan here... village would have to be successfull before
        // todo added !storyFlags.Any(x => x.Key.StartsWith("TradeCaravan_") && x.Value == false) && to both sides of the condition, check if that works alright
        private bool CheckCanStartCountDownOnNewShrine() =>
            //!storyFlags.Any(x => x.Key.StartsWith("TradeCaravan_") && x.Value == false) &&
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            || 
            //!storyFlags.Any(x => x.Key.StartsWith("TradeCaravan_") && x.Value == false) && 
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == true)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true);

        // todo - incomplete
        private bool CheckCanStartFriendlyCaravanCounter() => !storyFlags["TradeCaravan_InitCountDownStarted"];
        private bool CheckCanStartVillageGenerationCounter() => storyFlags["TradeCaravan_DialogFinished"] && !storyFlags["IntroVillage_InitCountDownStarted"];
        private bool CheckCanStartApocalypse() => storyFlags["Shrine1_Completed"] && ModSettings.Get().apocalypseEnabled && !storyFlags["Judgment_ApocalypseStarted"];

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

