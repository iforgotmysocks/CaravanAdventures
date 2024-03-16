using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Patches
{
    public class CaravanCamp
    {
        public static void ApplyPatches()
        {
            // fixing shelf tents not being able to link a new storage group due to nre in vanilla code when no map is active
            var org = AccessTools.Method(typeof(StorageGroupManager), nameof(StorageGroupManager.NewStorageName));
            var pre = new HarmonyMethod(typeof(CaravanCamp).GetMethod(nameof(NewStorageNamePrefix)));
            HarmonyPatcher.harmony.Patch(org, pre, null);
        }

        public static bool NewStorageNamePrefix(StorageGroupManager __instance, ref string __result)
        {
            if (!ModSettings.caravanCampEnabled) return true;
            if (Current.Game?.CurrentMap?.zoneManager != null) return true;
            DLog.Message($"Returning name that was requested without a current map selected");
            __result = "StorageGroup".Translate();
            return false;
        }
    }
}
