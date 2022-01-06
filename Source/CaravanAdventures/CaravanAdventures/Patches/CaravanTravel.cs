using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld.Planet;
using CaravanAdventures.CaravanImprovements;

namespace CaravanAdventures.Patches
{
    class CaravanTravel
    {
        public static void ApplyPatches()
        {
            if (!ModSettings.caravanCampEnabled) return;
            var carTravelOrg = AccessTools.PropertyGetter(typeof(Caravan), nameof(Caravan.NightResting));
            var carTravelPost = new HarmonyMethod(typeof(CaravanTravel).GetMethod(nameof(CarTravelPostfix)));
            HarmonyPatcher.harmony.Patch(carTravelOrg, null, carTravelPost);
        }

        public static void CarTravelPostfix(ref bool __result, Caravan __instance)
        {
            var decisions = __instance.GetComponent<CompCaravanDecisions>();
            if ((decisions?.allowNightTravel ?? false) && !CaravanNeedsResting(__instance)) __result = false;
        }

        private static bool CaravanNeedsResting(Caravan caravan)
        {
            return caravan.pawns.InnerListForReading.Where(x => x?.needs?.rest?.CurLevelPercentage != null).Any(x => x?.needs?.rest.CurLevelPercentage <= 0.01);
        }
    }
}
