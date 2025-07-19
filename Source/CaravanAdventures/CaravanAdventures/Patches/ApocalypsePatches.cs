using CaravanAdventures.CaravanStory;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class ApocalypsePatches
    {
        public static void ApplyPatches()
        {
            var seasonTempOrg = AccessTools.Method(typeof(GenTemperature), nameof(GenTemperature.OffsetFromSeasonCycle));
            var seasonTempPost = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(OffsetFromSeasonCycle_Postfix)));
            HarmonyPatcher.harmony.Patch(seasonTempOrg, null, seasonTempPost);
            // GenTemperature / GetTemperatureFromSeasonAtTile

            var tempstringOrg = AccessTools.PropertyGetter(typeof(WorldInspectPane), "TileInspectString");
            var tempstringPost = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(WorldInspectPane_TileInspectString_Postfix)));
            HarmonyPatcher.harmony.Patch(tempstringOrg, null, tempstringPost);


            var aggregateOrg = AccessTools.Method(typeof(GameConditionManager), "AggregateTemperatureOffset");
            //var aggregatePrefix = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(Aggregate_Prefix)));
            var aggregatePost = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(Aggregate_Postfix)));
            HarmonyPatcher.harmony.Patch(aggregateOrg, null, aggregatePost);

            //var excludedOrg = AccessTools.Method(typeof(GameConditionManager), "MapExcludedByFilter");
            //var excludedPost = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(excluded_Postfix)));
            //HarmonyPatcher.harmony.Patch(excludedOrg, null, excludedPost);
        }

        private static Pawn _targetPawn = null;
        private static int _logCount = 0;

        // todo 1.6 remove, only used for debugging
        //public static bool Aggregate_Prefix(ref float __result, GameConditionManager __instance)
        //{
        //    DLog.Message($"before aggregating result: {__result}");
        //    return true;
        //}

        public static GameCondition_Apocalypse CachedApo { get; set; }

        public static void Aggregate_Postfix(ref float __result, GameConditionManager __instance)
        {
            // todo 1.6 add check for space, we only want that applied on planetmaps
            if (__instance.ownerMap == null) return;
            if (CachedApo == null) CachedApo = Find.World.GameConditionManager.ActiveConditions.FirstOrDefault(x => x.def == StoryDefOf.CAGameCondition_Apocalypse) as GameCondition_Apocalypse;
            if (CachedApo == null) return;
            if (!CachedApo.Active)
            {
                CachedApo = null;
                return;
            }
            __result += CachedApo.TemperatureOffset();
        }

        public static void OffsetFromSeasonCycle_Postfix(ref float __result, PlanetTile tile)
        {
            if (CompCache.StoryWC?.questCont?.LastJudgment?.Apocalypse == null) return;
            Map map = Current.Game.FindMap(tile);
            if (map != null) return;
            __result += CompCache.StoryWC.questCont.LastJudgment.Apocalypse.TempOffset;
        }

        public static void WorldInspectPane_TileInspectString_Postfix(WorldInspectPane __instance, ref string __result)
            => __result += $"\n{"CAStoryCurrentTemperature".Translate()} {GenTemperature.GetTemperatureAtTile(Find.WorldSelector.SelectedTile).ToStringTemperature("F1")}";

    }
}