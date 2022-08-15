using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace CaravanAdventures.Patches.Compatibility
{
    class VFECoreFriendlyFactionDetectionPatch
    {
        public static void ApplyPatches(Assembly assembly)
        {
            if (!ModsConfig.RoyaltyActive) return;
            var org = AccessTools.Method(assembly.GetType("VFECore.Patch_GameComponentUtility+LoadedGame"), "Validator");
            var post = new HarmonyMethod(typeof(VFECoreFriendlyFactionDetectionPatch).GetMethod(nameof(VFECore_Patch_GameComponentUtility_LoadedGame_Validator_Postfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static void VFECore_Patch_GameComponentUtility_LoadedGame_Validator_Postfix(FactionDef faction, ref bool __result)
        {
            if (faction?.defName != "CAFriendlyMechanoid") return;
            __result = false;
        }

    }
}