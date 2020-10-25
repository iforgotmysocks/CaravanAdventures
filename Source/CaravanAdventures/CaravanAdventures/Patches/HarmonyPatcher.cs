using HarmonyLib;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            var harmony = new Harmony("iforgotmysocks.CaravanAdventures");
            CaravanTravel.ApplyPatches(harmony);
            AutomaticItemSelection.ApplyPatches(harmony);
            AbilityNeurotrainerDefGenerator.ApplyPatches(harmony);
            TalkPawnGUIOverlay.ApplyPatches(harmony);
        }
    }
}
