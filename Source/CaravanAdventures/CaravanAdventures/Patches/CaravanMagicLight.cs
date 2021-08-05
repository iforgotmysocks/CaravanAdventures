using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;
using RimWorld.Planet;
using CaravanAdventures.CaravanImprovements;
using System.Collections.Generic;
using System.Text;

namespace CaravanAdventures.Patches
{
    class CaravanMagicLight
    {
        public static List<Pawn> magicLightTravelers = new List<Pawn>();
        public static Caravan caravan;
        public static void ApplyPatches()
        {
            if (!ModSettings.storyEnabled) return;

            // patches not needed in 1.2 as the caravan speed is still calculated by actual pawn speed

            //var orgt = AccessTools.Method(typeof(CaravanTicksPerMoveUtility), "GetTicksPerMove", new System.Type[] { typeof(Caravan), typeof(StringBuilder) });
            //var pret = new HarmonyMethod(typeof(CaravanMagicLight).GetMethod(nameof(GetTicksPerMovePrefix)));
            //HarmonyPatcher.harmony.Patch(orgt, pret, null);

            //var org = AccessTools.Method(typeof(CaravanTicksPerMoveUtility), "BaseHumanlikeTicksPerCell");
            //var post = new HarmonyMethod(typeof(CaravanMagicLight).GetMethod(nameof(BaseHumanlikeTicksPerCellPostfix)));
            //HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static bool GetTicksPerMovePrefix(Caravan caravan, StringBuilder explanation)
        {
            CaravanMagicLight.caravan = caravan;
            return true;
        }

        public static void BaseHumanlikeTicksPerCellPostfix(ref int __result)
        {
            if (magicLightTravelers.NullOrEmpty() || caravan == null || !ModSettings.storyEnabled) return;
            if (!caravan.PawnsListForReading.Any(pawn => magicLightTravelers.Contains(pawn))) return;
            __result =  UnityEngine.Mathf.RoundToInt(__result / ModSettings.magicLightCaravanSpeedMult);
        }
    }
}
