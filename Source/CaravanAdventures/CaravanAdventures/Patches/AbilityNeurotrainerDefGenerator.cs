using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using RimWorld;

namespace CaravanAdventures.Patches
{
    class AbilityNeurotrainerDefGenerator
    {
        public static void ApplyPatches(Harmony harmony)
        {
            var org = AccessTools.Method(typeof(NeurotrainerDefGenerator), "ImpliedThingDefs");
            var post = new HarmonyMethod(typeof(Patches.AbilityNeurotrainerDefGenerator), nameof(ImpliedThingDefs_Postfix));
            harmony.Patch(org, null, post);
        }

        public static void ImpliedThingDefs_Postfix(ref IEnumerable<ThingDef> __result)
        {
            var newList = __result.ToList();
            newList.RemoveAll(x => x.defName.ToLower().StartsWith("psytrainer_ancient"));
            __result = newList;
        }
    }
}
