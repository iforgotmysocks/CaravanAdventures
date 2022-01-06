using HarmonyLib;
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
        }

        public static void OffsetFromSeasonCycle_Postfix(ref float __result, int tile)
        {
            if (CompCache.StoryWC?.questCont?.LastJudgment?.Apocalypse == null) return;
            Map map = Current.Game.FindMap(tile);
            if (map != null) return;
            __result += CompCache.StoryWC.questCont.LastJudgment.Apocalypse.TempOffset;
        }


    }
}