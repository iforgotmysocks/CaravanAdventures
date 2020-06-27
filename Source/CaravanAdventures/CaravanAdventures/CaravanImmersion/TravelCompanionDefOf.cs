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

        public static PawnRelationDef RelationNamed(string defName)
        {
            return DefDatabase<PawnRelationDef>.GetNamed(defName, true);
        }

        public static ThoughtDef ThoughtNamed(string defName)
        {
            return DefDatabase<ThoughtDef>.GetNamed(defName, true);
        }

        public static TravelCompanionDef Outsider;
        public static TravelCompanionDef ThirdWheel;
        public static TravelCompanionDef FamiliarFool;
        public static TravelCompanionDef PartOfTheGang;
        public static TravelCompanionDef TrustedMate;
        public static TravelCompanionDef TrueFriend;

        public static PawnRelationDef OutsiderRelation;
        public static PawnRelationDef ThirdWheelRelation;
        public static PawnRelationDef FamiliarFoolRelation;
        public static PawnRelationDef PartOfTheGangRelation;
        public static PawnRelationDef TrustedMateRelation;
        public static PawnRelationDef TrueFriendRelation;

    }
}
