using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Emit;

namespace CaravanAdventures.Patches.Compatibility
{
    class SoS2Patch
    {
        private static Assembly assembly;
        public static string SoS2AssemblyName = "shipshaveinsides";
        public static string OuterSpaceBiomeName = "OuterSpaceBiome";

        private static Type compShipHeatSourceType;
        private static FieldInfo parentPropInfo;
        //private static MethodInfo addHeatMethodInfo;

        private static Pawn capableAuraPawn;

        public static void ApplyPatches(Assembly assembly)
        {
            SoS2Patch.assembly = assembly;
            if (ModSettings.sos2AuraHeatManagementEnabled)
            {
                var addHeatOrg = AccessTools.Method(assembly.GetType("RimWorld.CompShipHeat"), "AddHeatToNetwork");
                var addHeatPre = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.CompShipHeatSource_AddHeatToNetwork_Prefix));
                HarmonyPatcher.harmony.Patch(addHeatOrg, addHeatPre, null);
                LoadReflectionNecessities();
            }
        }

        internal static void Reset()
        {
            capableAuraPawn = null;
        }

        private static void LoadReflectionNecessities()
        {
            compShipHeatSourceType = assembly.GetType("RimWorld.CompShipHeatSource");
            parentPropInfo = compShipHeatSourceType.BaseType.BaseType.GetField("parent", BindingFlags.Instance | BindingFlags.Public);
            //addHeatMethodInfo = compShipHeatSourceType.GetMethod("AddHeatToNetwork", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void CompShipHeatSource_AddHeatToNetwork_Prefix(object __instance, ref float amount)
        {
            if (amount < 15) return;

            var parentValue = (ThingWithComps)parentPropInfo.GetValue(__instance);
            var map = parentValue?.Map;
            if (map?.ParentFaction != Faction.OfPlayerSilentFail) return;
            var remainingAmount = amount * ((100 - ModSettings.sos2HeatAbsorptionPercentage) / 100);
            var calcedHeat = (amount - remainingAmount) / ModSettings.sos2AuraHeatMult;

            if (capableAuraPawn == null
                || capableAuraPawn.Dead
                || capableAuraPawn.Destroyed
                || capableAuraPawn.health.hediffSet.HasHediff(HediffDefOf.PsychicShock)
                || capableAuraPawn?.Map != map
                || !CaravanStory.StoryUtility.IsAuraProtectedAndTakesShipHeat(capableAuraPawn)
                || capableAuraPawn.psychicEntropy.limitEntropyAmount && (capableAuraPawn.psychicEntropy.EntropyValue + calcedHeat) > capableAuraPawn.psychicEntropy.MaxEntropy)
            {
                capableAuraPawn = null;
            }

            capableAuraPawn = capableAuraPawn
                ?? (capableAuraPawn = CaravanStory.StoryUtility.GetFirstPawnWith(map, x
                    => x.HasPsylink
                        && !x.health.hediffSet.HasHediff(HediffDefOf.PsychicShock)
                        && CaravanStory.StoryUtility.IsAuraProtectedAndTakesShipHeat(x)
                        && ((x.psychicEntropy.EntropyValue + calcedHeat) <= x.psychicEntropy.MaxEntropy || !x.psychicEntropy.limitEntropyAmount)));

            if (capableAuraPawn == null) return;

            if (!capableAuraPawn.psychicEntropy.TryAddEntropy(calcedHeat, null, true, !capableAuraPawn.psychicEntropy.limitEntropyAmount))
            {
                capableAuraPawn = null;
                return;
            }
            amount = remainingAmount;
        }

        internal static bool Installed() => ModSettings.storyEnabled && CompatibilityPatches.InDetectedAssemblies("shipshaveinsides");
    }
}