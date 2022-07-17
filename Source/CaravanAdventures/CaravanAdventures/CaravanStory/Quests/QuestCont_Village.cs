using RimWorld.Planet;
using System;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_Village : IExposable
    {
        private Pawn storyContact;
        private StoryVillageMP settlement;
        private MapParent destroyedSettlement;
        public float villageGenerationCounter = -1f;
        private bool respawnVillage;

        public Pawn StoryContact { get => storyContact; set => storyContact = value; }
        public StoryVillageMP Settlement { get => settlement; internal set => settlement = value; }
        public MapParent DestroyedSettlement { get => destroyedSettlement; internal set => destroyedSettlement = value; }
        public float BaseDelayVillageGeneration => Helper.Debug() ? 1000f : (60000f * 5f);

        public bool RespawnVillage { get => respawnVillage; private set => respawnVillage = value; }

        public QuestCont_Village()
        {
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref storyContact, "storyContact");
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_References.Look(ref destroyedSettlement, "destroyedSettlement");
            Scribe_Values.Look(ref villageGenerationCounter, "villageGenerationCounter");
            Scribe_Values.Look(ref respawnVillage, "respawnVillage", false);
        }

        public void TryCheckVillageAndEnsure()
        {
            if (Settlement != null && settlement.Faction == StoryUtility.FactionOfSacrilegHunters) return;
            if (villageGenerationCounter > 0f) return;
            villageGenerationCounter = 0f;
            respawnVillage = true;
            //CaravanStory.StoryUtility.GenerateFriendlyVillage(ref villageGenerationCounter, true);
        }
    }
}
