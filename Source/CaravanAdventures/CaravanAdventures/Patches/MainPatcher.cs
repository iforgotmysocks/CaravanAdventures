using HarmonyLib;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    static class MainPatcher
    {
        static MainPatcher()
        {
            
        }

        public static void Patch()
        {
            var harmony = new Harmony("iforgotmysocks.CaravanAdventures");
            CaravanTravel.ApplyPatches(harmony);
            AutomaticItemSelection.ApplyPatches(harmony);
            AbilityNeurotrainerDefGenerator.ApplyPatches(harmony);
            TalkPawnGUIOverlay.ApplyPatches(harmony);
        }
    }
}
