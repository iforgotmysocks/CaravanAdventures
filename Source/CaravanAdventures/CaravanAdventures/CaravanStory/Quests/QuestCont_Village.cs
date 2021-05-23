using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_Village : IExposable
    {
        private Pawn storyContact;
        private StoryVillageMP settlement;
        private MapParent destroyedSettlement;
        public float villageGenerationCounter = -1f;
        public Pawn StoryContact { get => storyContact; set => storyContact = value; }
        public StoryVillageMP Settlement { get => settlement; internal set => settlement = value; }
        public MapParent DestroyedSettlement { get => destroyedSettlement; internal set => destroyedSettlement = value; }
        public float BaseDelayVillageGeneration => Helper.Debug() ? 1000f : (60000f * 5f);

        public QuestCont_Village()
        {
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref storyContact, "storyContact");
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_References.Look(ref destroyedSettlement, "destroyedSettlement");
            Scribe_Values.Look(ref villageGenerationCounter, "villageGenerationCounter");
        }
    }
}
