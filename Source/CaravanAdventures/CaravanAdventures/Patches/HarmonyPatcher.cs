using HarmonyLib;
using System;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    static class HarmonyPatcher
    {
        public static Harmony harmony;
        static HarmonyPatcher()
        {
            harmony = harmony ?? (harmony = new Harmony("iforgotmysocks.CaravanAdventures"));
            if (ModSettings.caravanCampEnabled) CaravanTravel.ApplyPatches(harmony);
            if (ModSettings.caravanFormingFilterSelectionEnabled) AutomaticItemSelection.ApplyPatches(harmony);
            if (ModSettings.bountyEnabled) KillBountyPatches.ApplyPatches(harmony);
            if (ModSettings.storyEnabled)
            {
                TalkPawnGUIOverlay.ApplyPatches(harmony);
                BossPatches.ApplyPatches(harmony);
            }

            // new todo 
            AbilityBackgroundPatch.ApplyPatches(harmony);
        }

        /// <summary>
        /// Early patches before regular static constructor patches, as they require defs to not be loaded yet
        /// </summary>
        internal static void RunEarlyPatches()
        {
            harmony = harmony ?? (harmony = new Harmony("iforgotmysocks.CaravanAdventures"));
            if (ModsConfig.RoyaltyActive) AbilityNeurotrainerDefGenerator.ApplyPatches(harmony);
        }
    }
}
