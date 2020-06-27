using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    [DefOf]
    public static class TravelCompanionDefOf
    {
        static TravelCompanionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TravelCompanionDefOf));
        }

        public static TravelCompanionDef ThirdWheel;
        public static TravelCompanionDef FamiliarFool;
        public static TravelCompanionDef PartOfTheGang;
        public static TravelCompanionDef TrustedMate;
        public static TravelCompanionDef TrueFriend;
    }
}
