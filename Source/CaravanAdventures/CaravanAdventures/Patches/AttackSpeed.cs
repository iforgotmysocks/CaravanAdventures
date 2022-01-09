using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;

namespace CaravanAdventures.Patches
{
    class AttackSpeed
    {
        public static void ApplyPatches()
        {
            if (!ModSettings.attackspeedIncreaseForAncientProtectiveAura) return;
            var org = AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new System.Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) });
            var post = new HarmonyMethod(typeof(AttackSpeed).GetMethod(nameof(AttackSpeedPostfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static void AttackSpeedPostfix(ref float __result, VerbProperties __instance, Tool tool, Pawn attacker, Thing equipment)
        {
            if (attacker == null 
                    || attacker?.Faction != Faction.OfPlayer
                    || !attacker.IsColonist
                    || !__instance.IsMeleeAttack
                    || attacker.health.hediffSet.hediffs.FirstOrDefault(x => x?.def == CaravanAbilities.AbilityDefOf.CAAncientGift) == null) return;

            __result *= ModSettings.attackspeedMultiplier;
        }

    }
}
