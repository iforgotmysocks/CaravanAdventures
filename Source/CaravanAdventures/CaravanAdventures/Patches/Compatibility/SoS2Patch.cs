using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Collections;

namespace CaravanAdventures.Patches.Compatibility
{
    class SoS2Patch
    {
        private static Assembly assembly;
        public static string SoS2AssemblyName = "shipshaveinsides";
        public static void ApplyPatches(Assembly assembly)
        {
            SoS2Patch.assembly = assembly;
            var org = AccessTools.Method(assembly.GetType("SaveOurShip2.ShipInteriorMod2"), "hasSpaceSuit");
            var postfix = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.ShipInteriorMod2_hasSpaceSuit_Postfix));
            HarmonyPatcher.harmony.Patch(org, null, postfix);

            var addHeatOrg = AccessTools.Method(assembly.GetType("RimWorld.CompShipHeatSource"), "AddHeatToNetwork");
            var addHeatPost = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.CompShipHeatSource_AddHeatToNetwork_Postfix));
            HarmonyPatcher.harmony.Patch(addHeatOrg, null, addHeatPost);
        }

        public static void ShipInteriorMod2_hasSpaceSuit_Postfix(ref bool __result, Pawn pawn)
        {
            if (__result == true || !CaravanStory.StoryUtility.IsAuraProtected(pawn)) return;
            __result = true;
        }

        public static void CompShipHeatSource_AddHeatToNetwork_Postfix(object __instance, float amount, bool remove = false)
        {
            if (remove) return;

            var compShipHeatSourceType = assembly.GetType("RimWorld.CompShipHeatSource");
            var parentPropInfo = compShipHeatSourceType.BaseType.BaseType.GetField("parent", BindingFlags.Instance | BindingFlags.Public);
            var parentValue = (ThingWithComps)parentPropInfo.GetValue(__instance);

            var map = parentValue?.Map;
            var calcedHeat = amount * ModSettings.sos2AuraHeatMult;
            var capableAuraPawn = CaravanStory.StoryUtility.GetFirstPawnWith(map, x => x.HasPsylink && !x.health.hediffSet.HasHediff(HediffDefOf.PsychicShock) && CaravanStory.StoryUtility.IsAuraProtectedAndTakesShipHeat(x) && (x.psychicEntropy.EntropyValue + calcedHeat <= x.psychicEntropy.MaxEntropy || !x.psychicEntropy.limitEntropyAmount));
            if (map == null || capableAuraPawn == null) return;

            if (!capableAuraPawn.psychicEntropy.TryAddEntropy(calcedHeat, null, true, capableAuraPawn.psychicEntropy.limitEntropyAmount)) return;

            var addHeatMethodInfo = compShipHeatSourceType.GetMethod("AddHeatToNetwork", BindingFlags.Public | BindingFlags.Instance);
            addHeatMethodInfo.Invoke(__instance, new object[] { amount, true });
        }

        /*
        public static void ShipHeatNet_Tick_Postfix(object __instance)
        {
            BiomeDef sos2Def = null;
            sos2Def = DefDatabase<BiomeDef>.GetNamed("OuterSpaceBiome", false);

            var playerSpaceMap = Find.Maps.Where(cmap => cmap.ParentFaction == Faction.OfPlayerSilentFail
                && (sos2Def != null && cmap.Biome == sos2Def))
                ?.FirstOrDefault();

            var netType = assembly.GetType("RimWorld.ShipHeatMapComp");
            var netComps = playerSpaceMap.components.Where(x => x.GetType() == netType);

            var shipHeatNetType = assembly.GetType("RimWorld.ShipHeatNet");
            var convertedInstance = Convert.ChangeType(__instance, shipHeatNetType);

            foreach (var netComp in netComps)
            {
                var listInfo = netComp.GetType().GetField("cachedNets", BindingFlags.Instance | BindingFlags.NonPublic);
                var listValue = (IEnumerable)listInfo.GetValue(netComp);

                foreach (var net in listValue)
                {
                    var convertedNet = Convert.ChangeType(net, shipHeatNetType);
                    if (net != convertedInstance) continue;

                    var field = shipHeatNetType.GetField("StorageCapacity", BindingFlags.Instance | BindingFlags.Public);
                    var currentValue = (float)field.GetValue(convertedInstance);
                    var newValue = currentValue + 200000;
                    field.SetValue(convertedInstance, newValue);
                }
            }
        }

        public static void CompShipCombatShield_HitShield_Postfix(object __instance, Projectile proj)
        {
            var shieldCompType = assembly.GetType("RimWorld.CompShipCombatShield");
            var parentPropInfo = shieldCompType.BaseType.GetField("parent", BindingFlags.Instance | BindingFlags.Public);
            var parentValue = (ThingWithComps)parentPropInfo.GetValue(__instance);

            var map = parentValue?.Map;
            if (map == null) return;
            if (!map.mapPawns.FreeColonists.Any(x => CaravanStory.StoryUtility.IsAuraProtected(x))) return;

            var heatToRemove = proj.DamageAmount * 1f * 1.5f;

            var compShipHeatSourceType = assembly.GetType("RimWorld.CompShipHeatSource");
            var heatComp = parentValue.AllComps.FirstOrDefault(x => x.GetType() == compShipHeatSourceType);

            var addHeatMethodInfo = compShipHeatSourceType.GetMethod("AddHeatToNetwork", BindingFlags.Public | BindingFlags.Instance);
            addHeatMethodInfo.Invoke(heatComp, new object[] { heatToRemove, true });
        }
        */
    }   
}