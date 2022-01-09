using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    public class TravelCompanionDef : Def
    {
        public int thoughtStage = -1;
        public float maxDays = -1;
        public string relationDefName;

        public static TravelCompanionDef Named(string defName)
        {
            return DefDatabase<TravelCompanionDef>.GetNamed(defName, true);
        }

    }
}
