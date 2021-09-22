﻿using HarmonyLib;
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
            if (ModSettings.caravanCampEnabled) CaravanTravel.ApplyPatches();
            if (ModSettings.caravanFormingFilterSelectionEnabled) AutomaticItemSelection.ApplyPatches();
            if (ModSettings.bountyEnabled) KillBountyPatches.ApplyPatches();
            if (ModSettings.storyEnabled)
            {
                TalkPawnGUIOverlay.ApplyPatches();
                BossPatches.ApplyPatches();
                CaravanMagicLight.ApplyPatches();
                AttackSpeed.ApplyPatches();
                // scythernotifyfix only required in 1-2
                ScytherNotifyFix.ApplyPatches();

                if (ModSettings.allowApocToAlterTileTemp) ApocalypsePatches.ApplyPatches();
            }

            if (ModSettings.spDecayLevelIncrease) SpDecayLevelIncrease.ApplyPatches();
            if (ModSettings.showLetterRemoval) LetterRemovalPatch.ApplyPatches();
        }

        /// <summary>
        /// Early patches before regular static constructor patches, as they require defs to not be loaded yet
        /// </summary>
        internal static void RunEarlyPatches()
        {
            harmony = harmony ?? (harmony = new Harmony("iforgotmysocks.CaravanAdventures"));
            if (ModsConfig.RoyaltyActive) AbilityNeurotrainerDefGenerator.ApplyPatches();
        }
    }
}
