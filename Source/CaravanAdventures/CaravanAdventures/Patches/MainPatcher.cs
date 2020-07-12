using HarmonyLib;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    static class MainPatcher
    {
        static MainPatcher()
        {
            var harmony = new Harmony("iforgotmysocks.CaravanAdventures");
            CaravanTravel.ApplyPatches(harmony);
            AutomaticItemSelection.ApplyPatches(harmony);
            AbilityNeurotrainerDefGenerator.ApplyPatches(harmony);
        }
    }
}
