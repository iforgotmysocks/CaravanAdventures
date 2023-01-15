using CaravanAdventures.CaravanAbilities;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Patches
{
    public class KeepApparelOnHostileMapsPatch
    {
        public static bool PatchApplied;
        public static void ApplyPatches()
        {
            if (!ModSettings.keepApparelOnHostileMaps) return;
            {
                var org = AccessTools.Method(typeof(OutfitForcedHandler), nameof(OutfitForcedHandler.AllowedToAutomaticallyDrop));
                var post = new HarmonyMethod(typeof(KeepApparelOnHostileMapsPatch).GetMethod(nameof(OutfitForcedHandler_AllowedToAutomaticallyDrop_Postfix)));
                HarmonyPatcher.harmony.Patch(org, null, post);
                PatchApplied = true;
            }
        }

        public static void OutfitForcedHandler_AllowedToAutomaticallyDrop_Postfix(OutfitForcedHandler __instance, ref bool __result, Apparel ap)
        {
            if (!__result || ap?.Wearer?.IsColonist != true || !ModSettings.keepApparelOnHostileMaps || ap?.Wearer?.Map?.IsPlayerHome == true) return;
            __result = false;
        }
    }
}
