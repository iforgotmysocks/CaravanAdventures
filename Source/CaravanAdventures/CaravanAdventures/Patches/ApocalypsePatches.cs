using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class ApocalypsePatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            //var org = AccessTools.Method(typeof(TileTemperaturesComp.CachedTileTemperatureData), "CalculateOutdoorTemperatureAtTile");
            var nestedType = typeof(TileTemperaturesComp).GetNestedType("CachedTileTemperatureData", BindingFlags.Static |
                                                   BindingFlags.Instance |
                                                   BindingFlags.Public |
                                                   BindingFlags.NonPublic);
            if (nestedType == null) DLog.Warning($"nope");
            //var methodInfo = nestedType.GetMethod()
            var org = AccessTools.Method(nestedType, "CalculateOutdoorTemperatureAtTile");
            //var org = AccessTools.Method(Type.GetType("CachedTileTemperatureData"), "CalculateOutdoorTemperatureAtTile");
            var post = new HarmonyMethod(typeof(ApocalypsePatches).GetMethod(nameof(CalculateOutdoorTemperatureAtTilePostFix)));
            harmony.Patch(org, null, post);
        }

        public static void CalculateOutdoorTemperatureAtTilePostFix(ref float __result)
        {
            if (CompCache.StoryWC?.questCont?.LastJudgment?.Apocalypse == null) return;
            DLog.Warning($"temp before adjusting: {__result}");
            __result += CompCache.StoryWC.questCont.LastJudgment.Apocalypse.TempOffset;
            DLog.Warning($"temp after adjusting: {__result}");
        }


    }
}