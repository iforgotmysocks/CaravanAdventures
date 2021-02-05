using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
#pragma warning disable CS0649
    [DefOf]

    class AbilityDefOf
    {
        [MayRequireRoyalty]
        public static HediffDef CAAncientProtectiveAura;
        [MayRequireRoyalty]
        public static HediffDef CAAncientGift;
        [MayRequireRoyalty]
        public static HediffDef CAAncientCoordinator;
        [MayRequireRoyalty]
        public static HediffDef CAAncientProtectiveAuraLinked;

        public static BodyPartDef Finger;
        public static BodyPartDef Toe;

        public static AbilityDef Named(string defName, bool errorOnFail = true)
        {
            return DefDatabase<AbilityDef>.GetNamed(defName, errorOnFail);
        }
    }
}
