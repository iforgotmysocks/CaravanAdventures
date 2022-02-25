using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using System.Reflection;
using System;

namespace CaravanAdventures.Patches.Compatibility
{
    class WinstonWavesPatch
    {
        public static void ApplyPatches(Assembly assembly)
        {
            if (!ModSettings.storyEnabled) return;
            var org = AccessTools.Method(assembly.GetType("VSEWW.IncidentWorker_Raid_TryExecuteWorker_Patch"), "Prefix");
            var pre = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix)));
            HarmonyPatcher.harmony.Patch(org, pre, null);

            var org2 = AccessTools.Method(assembly.GetType("VSEWW.IncidentWorker_Raid_TryGenerateRaidInfo_Patch"), "Prefix");
            var pre2 = new HarmonyMethod(typeof(WinstonWavesPatch).GetMethod(nameof(VSEWW_IncidentWorker_Raid_TryExecuteWorker_And_TryGenerateRaidInfo_Prefix_Prefix)));
            HarmonyPatcher.harmony.Patch(org2, pre2, null);
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
