using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    public class TravelCompanionDef : Def
    {
        public int thoughtStage = -1;
        public float maxDays = -1;

        public static TravelCompanionDef Named(string defName)
        {
            return DefDatabase<TravelCompanionDef>.GetNamed(defName, true);
        }

    }
}
