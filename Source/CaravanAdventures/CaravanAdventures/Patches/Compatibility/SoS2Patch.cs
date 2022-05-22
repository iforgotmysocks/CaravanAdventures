using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace CaravanAdventures.Patches.Compatibility
{
    class SoS2Patch
    {
        public static void ApplyPatches(Assembly assembly)
        {
            var org = AccessTools.Method(assembly.GetType("SaveOurShip2.ShipInteriorMod2"), "hasSpaceSuit");
            var postfix = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.ShipInteriorMod2_hasSpaceSuit_Postfix));
            HarmonyPatcher.harmony.Patch(org, null, postfix);
        }

        public static void ShipInteriorMod2_hasSpaceSuit_Postfix(ref bool __result, Pawn pawn)
        {
            if (__result == true || !CaravanStory.StoryUtility.IsAuraProtected(pawn)) return;
            __result = true;
        }
    }   
}