using CaravanAdventures.CaravanStory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures
{
    // when adding new components, remember to set them null in FinalizeInit()
    static class CompCache
    {
        private static StoryWC storyWC;
        public static StoryWC StoryWC { get => storyWC ?? (storyWC = Find.World.GetComponent<StoryWC>()); set => storyWC = value; }
    }
}
