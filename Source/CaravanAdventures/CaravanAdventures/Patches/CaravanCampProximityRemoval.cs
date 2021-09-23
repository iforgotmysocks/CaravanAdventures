using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;
using RimWorld.Planet;
using CaravanAdventures.CaravanImprovements;
using System.Collections.Generic;

namespace CaravanAdventures.Patches
{
    class CaravanCampProximityRemoval
    {
        public static void ApplyPatches()
        {
            if (!ModSettings.caravanCampEnabled) return;
            var org = AccessTools.Method(typeof(SettlementProximityGoodwillUtility), nameof(SettlementProximityGoodwillUtility.AppendProximityGoodwillOffsets));
            var post = new HarmonyMethod(typeof(CaravanCampProximityRemoval).GetMethod(nameof(AppendProximityGoodwillOffsetsPostfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static void AppendProximityGoodwillOffsetsPostfix(int tile, List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
        {
            var isCamp = Find.World?.worldObjects?.SettlementAt(tile)?.Map?.listerBuildings?.allBuildingsColonist?.FirstOrDefault(y => y?.def?.defName == "CACampControl") != null;
            if (!isCamp) return;
            outOffsets.Clear();
            DLog.Message($"Removed all settlement prox offsets due to camp");
        }
    }
}
