using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class StoryWC : WorldComponent
    {
        public StoryWC(World world) : base(world)
        { }

        public static Dictionary<string, bool> storyFlags = new Dictionary<string, bool>()
        {
            { "Start_InitialTreeWhisper", false },
            { "Start_InitialTreeAddTalkOption", false },
            { "Start_MapTreeWhisper", false },
            { "Start_RechedTree", false },
            { "Start_CanReceiveGift", false },
            { "Start_ReceivedGift", false }
        };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref StoryWC.storyFlags, "storyFlags", LookMode.Value);
        }
    }
}
