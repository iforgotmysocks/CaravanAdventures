using CaravanAdventures.CaravanAbilities;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Patches
{
    public class EngageMeleePatches
    {
        public static void ApplyPatches()
        {
            if (!ModSettings.enableEngageMeleeFeature) return;
            {
                var org = AccessTools.PropertySetter(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted));
                var post = new HarmonyMethod(typeof(EngageMeleePatches).GetMethod(nameof(Pawn_DraftController_Set_Drafted_Postfix)));
                HarmonyPatcher.harmony.Patch(org, null, post);
            }
        }

        public static void Pawn_DraftController_Set_Drafted_Postfix(Pawn_DraftController __instance, ref bool __0)
        {
            if (!__0) return;
            var comp = __instance.pawn.GetComp<CompEngageMelee>();
            if (comp == null) return;
            if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.Disabled) comp.Enabled = false;
            else if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.Enabled) comp.Enabled = true; 
        }
    }
}
