using CaravanAdventures.CaravanMechBounty;
using CaravanAdventures.CaravanStory;
using Verse;

namespace CaravanAdventures
{
    // when adding new game / world components, remember to set them null in their own FinalizeInit() method
    static class CompCache
    {
        private static InitGC initGC;
        public static InitGC InitGC { get => initGC ?? (initGC = Current.Game?.GetComponent<InitGC>()); set => initGC = value; }

        private static StoryWC storyWC;
        public static StoryWC StoryWC { get => storyWC ?? (storyWC = Find.World?.GetComponent<StoryWC>()); set => storyWC = value; }
        
        private static BountyWC bountyWC;
        public static BountyWC BountyWC { get => bountyWC ?? (bountyWC = Find.World?.GetComponent<BountyWC>()); set => bountyWC = value; }
    }
}
