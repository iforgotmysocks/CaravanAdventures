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

        public Pawn StoryContact { get => storyContact; set => storyContact = value; }
        public StoryVillageMP Settlement { get => settlement; internal set => settlement = value; }
        public MapParent DestroyedSettlement { get => destroyedSettlement; internal set => destroyedSettlement = value; }

        public QuestCont_Village()
        {

        }

        public void ExposeData()
        {
            // todo - deep or ref? - what keeps the comp?
            Scribe_References.Look(ref storyContact, "storyContact");
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_References.Look(ref destroyedSettlement, "destroyedSettlement");
        }
    }
}
