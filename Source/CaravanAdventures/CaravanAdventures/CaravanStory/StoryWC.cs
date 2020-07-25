using RimWorld;
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
        public StoryWC(World world) : base(world)
        { }

        // todo 60000 * 2
        private float baseDelayNextShrineReveal = 600f * 2f;
        private float shrineRevealCounter = -1f;
        private int ticks = 0;
        private float countShrinesCompleted = 0f;
        private static readonly IntRange timeoutDaysRange = new IntRange(10, 12);
        private static readonly IntRange shrineDistance = new IntRange(2, 4); // 40,50

        public static Dictionary<string, bool> storyFlags = new Dictionary<string, bool>()
        {
            { "Start_InitialTreeWhisper", true },
            { "Start_InitialTreeAddTalkOption", true },
            { "Start_CanReceiveGift", true },
            { "Start_ReceivedGift", true },

            { "Shrine1_InitCountDownStarted", false },
            { "Shrine1_Created", false },
            { "Shrine1_Completed", false }
        };

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (ticks >= 60)
            {
                if (CheckCanStartCountDownOnNewShrine())
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

        private void TryCreateNewShrine()
        {
            for (var i = 0; i < 5; i++)
            {
                int tile;
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
        }

        private string BuildCurrentShrinePrefix() => "Shrine" + (countShrinesCompleted + 1) + "_";
        private bool CheckCanStartCountDownOnNewShrine() => !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false) && countShrinesCompleted == 0 && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true)
            || !storyFlags.Any(x => x.Key.StartsWith("Start_") && x.Value == false) && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "Completed" && x.Value == false) && !storyFlags.Any(x => x.Key == BuildCurrentShrinePrefix() + "InitCountDownStarted" && x.Value == true);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref storyFlags, "storyFlags", LookMode.Value);
        }


    }
}
