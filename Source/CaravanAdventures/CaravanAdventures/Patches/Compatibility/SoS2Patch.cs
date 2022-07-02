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
        private static MethodInfo addHeatMethodInfo;

        private static Pawn capableAuraPawn;

        public static void ApplyPatches(Assembly assembly)
        {
            SoS2Patch.assembly = assembly;
            if (ModSettings.sos2AuraPreventsHypoxia)
            {
                var org = AccessTools.Method(assembly.GetType("SaveOurShip2.ShipInteriorMod2"), "hasSpaceSuit");
                var postfix = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.ShipInteriorMod2_hasSpaceSuit_Postfix));
                HarmonyPatcher.harmony.Patch(org, null, postfix);
            }

            if (ModSettings.sos2AuraHeatManagementEnabled)
            {
                var addHeatOrg = AccessTools.Method(assembly.GetType("RimWorld.CompShipHeatSource"), "AddHeatToNetwork");
                var addHeatPre = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.CompShipHeatSource_AddHeatToNetwork_Prefix));
                HarmonyPatcher.harmony.Patch(addHeatOrg, addHeatPre, null);
                var addGiz = AccessTools.Method(typeof(Pawn), "GetGizmos");
                var addGizPost = new HarmonyMethod(typeof(SoS2Patch), nameof(SoS2Patch.Pawn_GetGizmos_Postfix));
                HarmonyPatcher.harmony.Patch(addGiz, null, addGizPost);
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
            addHeatMethodInfo = compShipHeatSourceType.GetMethod("AddHeatToNetwork", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void ShipInteriorMod2_hasSpaceSuit_Postfix(ref bool __result, Pawn pawn)
        {
            if (__result == true || !CaravanStory.StoryUtility.IsAuraProtected(pawn)) return;
            __result = true;
        }

        public static void CompShipHeatSource_AddHeatToNetwork_Prefix(object __instance, ref float amount, bool remove = false)
        {
            if (remove || amount < 6) return;

            var parentValue = (ThingWithComps)parentPropInfo.GetValue(__instance);
            var map = parentValue?.Map;
            if (map?.ParentFaction != Faction.OfPlayerSilentFail) return;
            var remainingAmount = amount * ((100 - ModSettings.sos2HeatAbsorptionPercentage) / 100);
            var calcedHeat = (amount - remainingAmount) / ModSettings.sos2AuraHeatMult;

            capableAuraPawn = capableAuraPawn ?? (capableAuraPawn = CaravanStory.StoryUtility.GetFirstPawnWith(map, x => x.HasPsylink && !x.health.hediffSet.HasHediff(HediffDefOf.PsychicShock) && CaravanStory.StoryUtility.IsAuraProtectedAndTakesShipHeat(x) && (x.psychicEntropy.EntropyValue + calcedHeat <= x.psychicEntropy.MaxEntropy || !x.psychicEntropy.limitEntropyAmount)));
            if (map == null
                || capableAuraPawn == null
                || capableAuraPawn.Dead
                || capableAuraPawn.Destroyed
                || capableAuraPawn.health.hediffSet.HasHediff(HediffDefOf.PsychicShock)
                || capableAuraPawn?.Map != map
                || !CaravanStory.StoryUtility.IsAuraProtectedAndTakesShipHeat(capableAuraPawn))
            {
                capableAuraPawn = null;
                return;
            }
            if (!capableAuraPawn.psychicEntropy.TryAddEntropy(calcedHeat, null, true, !capableAuraPawn.psychicEntropy.limitEntropyAmount))
            {
                capableAuraPawn = null;
                return;
            }
            amount = remainingAmount;
        }

        public static IEnumerable<Gizmo> Pawn_GetGizmos_Postfix(IEnumerable<Gizmo> list, Pawn __instance)
        {
            foreach (var existing in list) if (existing != null) yield return existing;
            var hediff = __instance.health.hediffSet.hediffs.FirstOrDefault(x => x.def?.defName == "CAAncientProtectiveAuraLinked" || x.def?.defName == "CAAncientProtectiveAura");
            if (hediff == null) yield break;
            var gizmoComp = hediff.TryGetComp<CaravanAdventures.CaravanAbilities.HediffComp_AncientProtectiveAura>();
            if (gizmoComp == null) yield break;
            foreach (var gizmo in gizmoComp.GetGizmos()) if (gizmo != null) yield return gizmo;
        }
    }




    /*
    public static void ShipHeatNet_Tick_Postfix(object __instance)
    {
        BiomeDef sos2Def = null;
        sos2Def = DefDatabase<BiomeDef>.GetNamed("Patches.Compatibility.SoS2Patch.OuterSpaceBiomeName", false);

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

<<<<<<< HEAD
        var addHeatMethodInfo = compShipHeatSourceType.GetMethod("AddHeatToNetwork", BindingFlags.Public | BindingFlags.Instance);
        addHeatMethodInfo.Invoke(heatComp, new object[] { heatToRemove, true });
    }
    */

    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, object __instance, ref float amount, bool remove = false)
    //        {
    //            var codes = new List<CodeInstruction>(instructions);

    //            var instructionsToAdd = new List<CodeInstruction>();
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Nop));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Ldarg_0));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Ldarg_0));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Call, typeof(SoS2Patch).GetMethod(nameof(CompShipHeatSource_AddHeatToNetwork_Prefix), BindingFlags.Public | BindingFlags.Static)));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Stloc_0));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Ldloc_0));
    //            instructionsToAdd.Add(new CodeInstruction(OpCodes.Brfalse_S, new Label() { LabelValue = }));
    //            var getLabelForInfo = AccessTools.Method(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(bool), typeof(bool) });
    //#pragma warning disable CS0252
    //            var idx = codes.FindIndex(code => code.operand == getLabelForInfo);
    //#pragma warning restore CS0252 

    //            if (idx == -1)
    //            {
    //                Log.Warning($"Could not find GetLabelFor code instruction; skipping changes");
    //                return instructions;
    //            }

    //            codes[idx].operand = AccessTools.Method(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(Pawn) });
    //            codes.RemoveRange(idx - 7, 7);

    //            return codes;
    //        }

}
