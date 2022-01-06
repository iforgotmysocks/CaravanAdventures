using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestPart_AddLocations : QuestPart
    {
        private List<WorldObject> locations;
        public List<WorldObject> Locations { get => locations; set => locations = value; }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref locations, "locations", LookMode.Reference);
        }

        public QuestPart_AddLocations()
        {
            locations = new List<WorldObject>();
        }

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets 
        { 
            get 
            {
                foreach (var orgTraget in base.QuestLookTargets) yield return orgTraget;
                foreach (var location in locations) yield return location;
            } 
        }
    }
}
