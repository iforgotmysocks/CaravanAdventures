﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        // todo 60000 * 2
        private float baseDelayNextShrineReveal = 600f * 2f;
        private static float shrineRevealCounter = -1f;
        private int ticks = -1;
        private static float countShrinesCompleted = 0f;
        private static readonly IntRange timeoutDaysRange = new IntRange(10, 12);
        private static readonly IntRange shrineDistance = new IntRange(2, 4); // 40,50

        public static Dictionary<string, bool> debugFlags = new Dictionary<string, bool>()
        {
            {"StoryStartDone", true },
            { "ShrinesDisabled", false }
        };

        public static Dictionary<string, bool> storyFlags = new Dictionary<string, bool>()
        {
            { "Start_InitialTreeWhisper", false },
            { "Start_InitialTreeAddTalkOption", false },
            { "Start_CanReceiveGift", false },
            { "Start_ReceivedGift", false },
        };

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (ticks == -1)
            {
                Log.Message($"Init story shrine flags");
                for (int i = 1; i < 6; i++)
                {
                    storyFlags["Shrine" + i + "_InitCountDownStarted"] = false;
                    storyFlags["Shrine" + i + "_Created"] = false;
                    storyFlags["Shrine" + i + "_Completed"] = false;
                }
            }

            if (debugFlags["StoryStartDone"])
                foreach (var flag in storyFlags.Where(x => x.Key.StartsWith("Start_")).ToList())
                    storyFlags[flag.Key] = true;
        }

        public StoryWC(World world) : base(world)
        {
        }

        //public override void FinalizeInit()
        //{
        //    base.FinalizeInit();
        //    Log.Message($"StoryWC FinallizeInit was called");
        //}

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (ticks >= 60)
            {
                if (CheckCanStartCountDownOnNewShrine() && !debugFlags["ShrinesDisabled"])
                {
                    shrineRevealCounter = baseDelayNextShrineReveal * (countShrinesCompleted + 1f) * 0.5f;
                    storyFlags[BuildCurrentShrinePrefix() + "InitCountDownStarted"] = true;
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

        public static string[] GetBossDefNames() => new string[] { "CACristalScythe", "CADevourer" };

        public static void IncreaseShrineCompleteCounter() => countShrinesCompleted++;

        private void TryCreateNewShrine()
        {
            int tile;
            // todo Create own find method that keeps the same distance from bases and caravans
            // -> after an unsuccesfull attempt, select tile that supports it for sure.
            if (!TileFinder.TryFindNewSiteTile(out tile, shrineDistance.min, shrineDistance.max))
            {
                shrineRevealCounter = 60f;
                Log.Message("Couldn't find tile to create a new shrine");
            }
            else
            {
                var ancientMasterShrineWO = (AncientMasterShrineWO)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.AncientMasterShrine);
                ancientMasterShrineWO.Tile = tile;
                //ancientMasterShrineWO.GetComponent<TimeoutComp>().StartTimeout(timeoutDaysRange.RandomInRange * 60000);
                Find.WorldObjects.Add(ancientMasterShrineWO);
                Find.LetterStack.ReceiveLetter("Story_Shrine1_NewShrineDetectedLetterLabel".Translate(), "Story_Shrine1_NewShrineDetectedLetterMessage".Translate(), LetterDefOf.PositiveEvent, ancientMasterShrineWO);
                storyFlags[BuildCurrentShrinePrefix() + "Created"] = true;
            }
        }

        public static void ResetCurrentShrineFlags() => storyFlags.Keys.Where(x => x.StartsWith(BuildCurrentShrinePrefix())).ToList().ForEach(key => storyFlags[key] = false);

        public static string BuildCurrentShrinePrefix() => "Shrine" + (countShrinesCompleted + 1) + "_";
        private bool CheckCanStartCountDownOnNewShrine() => 
            !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false) 
            && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" 
            && x.Value == true)
            || !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false) 
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == false) 
            && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storyFlags, "storyFlags", LookMode.Value);
            Scribe_Values.Look(ref ticks, "ticks");
        }


    }
}
