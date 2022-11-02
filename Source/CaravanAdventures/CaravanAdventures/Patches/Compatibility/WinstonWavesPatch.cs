using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using System.Reflection;
using System;
using CaravanAdventures.CaravanStory;

namespace CaravanAdventures.Patches.Compatibility
{
    class WinstonWavesPatch
    {
        public static void ApplyPatches(Assembly assembly)
        {
            if (!ModSettings.storyEnabled && !ModSettings.allowWinstonWaveFightsOnCamps) return;

            //var org = AccessTools.Method(assembly.GetType("VSEWW.IncidentWorker_Raid_TryExecuteWorker_Patch"), "Prefix");
            //var pre = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix)));
            //HarmonyPatcher.harmony.Patch(org, pre, null);

            //var org2 = AccessTools.Method(assembly.GetType("VSEWW.IncidentWorker_Raid_TryGenerateRaidInfo_Patch"), "Prefix");
            //var pre2 = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix)));
            //HarmonyPatcher.harmony.Patch(org2, pre2, null);

            var org = AccessTools.Method(assembly.GetType("VSEWW.MapComponent_Winston"), "FinalizeInit");
            var post = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_MapComponent_Winston_FinalizeInit_Postfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);

            //var org2 = AccessTools.Method(assembly.GetType("VSEWW.IncidentWorker_Raid_TryGenerateRaidInfo_Patch"), "Prefix");
            //var pre2 = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix)));
            //HarmonyPatcher.harmony.Patch(org2, pre2, null);
        }

        public static void VSEWW_MapComponent_Winston_FinalizeInit_Postfix(MapComponent __instance)
        {
            if (Find.Storyteller?.def?.defName != "VSE_WinstonWave") return;
            DLog.Message($"wwtest: {__instance?.map?.Parent?.def.defName} type: {__instance.GetType().Name}");
            if (!ModSettings.allowWinstonWaveFightsOnCamps) StopWWEventsOnCamps(__instance);
            if (!ModSettings.storyEnabled) return;
            StopWWEventsOnStoryLocations(__instance);
        }

        private static void StopWWEventsOnStoryLocations(MapComponent instance)
        {
            var mapParentDef = instance?.map?.Parent?.def;
            if (mapParentDef != null
                    && (new[] {
                        CaravanStory.CaravanStorySiteDefOf.CAStoryVillageMP,
                        CaravanStory.CaravanStorySiteDefOf.CAAncientMasterShrineMP,
                        CaravanStory.CaravanStorySiteDefOf.CALastJudgmentMP })
                        .Contains(mapParentDef))
            {
                DLog.Message($"removing ww comp from CA story map");
                instance.map.components.Remove(instance);
            }

            // todo check if we still need to exclude hunters from facitons that can raid the player
            //  return parms?.faction == CaravanStory.StoryUtility.FactionOfSacrilegHunters
            //         && !CaravanStory.StoryUtility.FactionOfSacrilegHunters.HostileTo(Faction.OfPlayer)

        }

        private static void StopWWEventsOnCamps(MapComponent __instance)
        {

            var control = __instance?.map?.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x?.def?.defName == "CACampControl");
            if (control == null)
            {
                DLog.Message($"didn't find control");
                return;
            }

            __instance.map.components.Remove(__instance);
        }

        public static bool VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix(IncidentParms parms, ref bool __result)
        {
            if (!CancelWWRaidInterruption(parms)) return true;
            __result = true;
            return false;
        }

        private static bool CancelWWRaidInterruption(IncidentParms parms)
        {
            if (Find.Storyteller?.def?.defName != "VSE_WinstonWave") return false;
            var mapParentDef = parms?.target is Map map ? map?.Parent?.def : null;
            return parms?.faction == CaravanStory.StoryUtility.FactionOfSacrilegHunters
                    && !CaravanStory.StoryUtility.FactionOfSacrilegHunters.HostileTo(Faction.OfPlayer)
                || mapParentDef != null
                    && (new[] {
                        CaravanStory.CaravanStorySiteDefOf.CAStoryVillageMP,
                        CaravanStory.CaravanStorySiteDefOf.CAAncientMasterShrineMP,
                        CaravanStory.CaravanStorySiteDefOf.CALastJudgmentMP })
                        .Contains(mapParentDef);
        }
    }
}
