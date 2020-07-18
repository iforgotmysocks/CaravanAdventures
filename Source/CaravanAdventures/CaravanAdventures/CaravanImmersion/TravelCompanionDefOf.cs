using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    public static class TravelCompanionDefOf
    {
        public static PawnRelationDef RelationNamed(string defName)
        {
            return DefDatabase<PawnRelationDef>.GetNamed(defName, true);
        }

        public static ThoughtDef ThoughtNamed(string defName)
        {
            return DefDatabase<ThoughtDef>.GetNamed(defName, true);
        }
    }
}
