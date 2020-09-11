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

namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        private readonly float baseDelayNextShrineReveal = Helper.Debug() ? 1800f : 60000f * 2f;
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
            { "ShrinesDisabled", false },
            { "DebugAllAbilities", false },
            { "VillageFinished", false },
            { "DebugResetVillagesAndShrines", false },
        };
        public Dictionary<string, bool> storyFlags;
        private List<string> flagsToAdd = new List<string>
        {
            "TradeCaravan_Arrived",
            "TradeCaravan_DialogFinished",

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
        };

        public QuestCont questCont;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storyFlags, "storyFlags", LookMode.Value);
            Scribe_Collections.Look(ref unlockedSpells, "unlockedSpells", LookMode.Def);
            Scribe_Collections.Look(ref mechBossKillCounters, "mechBossKillCounters", LookMode.Value);
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref shrineRevealCounter, "shrineRevealCounter");
            Scribe_Values.Look(ref countShrinesCompleted, "countShrinesCompleted");
            Scribe_Values.Look(ref bossMissedCounter, "bossMissedCounter");
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
            // todo check if saving the main girl here is responsible for the comp to not be available after load
            if (questCont == null) questCont = new QuestCont();
            if (questCont.Village == null)
            {
                Log.Message($"QuestComp Village was null");
                questCont.Village = new QuestCont_Village();
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            RunDebugActionsOnceAtStartUp();
            if (ticks == -1) StoryUtility.EnsureSacrilegHunters();

            if (ticks > 1200)
            {
                StoryUtility.GenerateStoryContact();
                StoryUtility.CheckCreateTradeCaravan();
                StoryUtility.GenerateFriendlyVillage();

                if (CheckCanStartCountDownOnNewShrine() && !debugFlags["ShrinesDisabled"])
                {
                    if (Dg) Log.Message("Shrine counter running" + shrineRevealCounter);
                    shrineRevealCounter = baseDelayNextShrineReveal * (countShrinesCompleted + 1f) * 0.5f;
                    SetShrineSF("InitCountDownStarted");
                }

                ticks = 0;
            }

            if (shrineRevealCounter == 0)
            {
                TryCreateNewShrine();
            }

            ticks++;
            shrineRevealCounter--;
        }

        private void RunDebugActionsOnceAtStartUp()
        {
            if (ranDebugActionsOnceAtStartUp) return; 

            if (debugFlags["DebugResetVillagesAndShrines"])
            {
                StoryUtility.RemoveExistingQuestFriendlyVillages();
                StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.AncientMasterShrineMP);
                StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.AncientMasterShrineWO);
                ResetCurrentShrineFlags();
                ResetSFsStartingWith("IntroVillage");
            }

            ranDebugActionsOnceAtStartUp = true;
        }

        public string[] GetBossDefNames() => new string[] { "CACristalScythe", "CABossMechDevourer" };

        public void IncreaseShrineCompleteCounter() => countShrinesCompleted++;

        private void TryCreateNewShrine()
        {
            // todo Create own find method that keeps the same distance from bases and caravans
            // -> after an unsuccesfull attempt, select tile that supports it for sure.
            if (!TileFinder.TryFindNewSiteTile(out var tile, shrineDistance.min, shrineDistance.max))
            {
                shrineRevealCounter = 60f;
                Log.Message("Couldn't find tile to create a new shrine");
            }
            else
            {
                var ancientMasterShrineWO = (AncientMasterShrineWO)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.AncientMasterShrineWO);
                ancientMasterShrineWO.Tile = tile;
                //ancientMasterShrineWO.GetComponent<TimeoutComp>().StartTimeout(timeoutDaysRange.RandomInRange * 60000);
                Find.WorldObjects.Add(ancientMasterShrineWO);
                Find.LetterStack.ReceiveLetter("Story_Shrine1_NewShrineDetectedLetterLabel".Translate(), "Story_Shrine1_NewShrineDetectedLetterMessage".Translate(), LetterDefOf.PositiveEvent, ancientMasterShrineWO);
                SetShrineSF("Created");
            }
        }

        // SF helper methods used to be static, if the CompCache doesn't work out, turn access to the SFs static again
        public bool Dg => debugFlags["ShowDebugInfo"];
        public void SetSF(string key) => storyFlags[key] = true;
        public void SetShrineSF(string postFix) => storyFlags[storyFlags.Keys.FirstOrDefault(x => x.StartsWith(BuildCurrentShrinePrefix() + postFix))] = true;
        public void ResetCurrentShrineFlags() => storyFlags.Keys.Where(x => x.StartsWith(BuildCurrentShrinePrefix())).ToList().ForEach(key => storyFlags[key] = false);
        public void ResetSFsStartingWith(string start) => storyFlags.Keys.Where(x => x.StartsWith(start)).ToList().ForEach(key => storyFlags[key] = false);
        public string BuildCurrentShrinePrefix() => "Shrine" + (countShrinesCompleted + 1) + "_";

        public List<AbilityDef> GetUnlockedSpells() => unlockedSpells;

        private bool CheckCanStartCountDownOnNewShrine() =>
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            || !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == true)
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true);

    }
}

