using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class SpDecayLevelIncrease
    {
        public static void ApplyPatches()
        {
            var skillOrg = AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Learn));
            var skillPost = new HarmonyMethod(typeof(SpDecayLevelIncrease).GetMethod(nameof(Learn_Postfix)));
            HarmonyPatcher.harmony.Patch(skillOrg, null, skillPost);

        }

        public static void Learn_Postfix(SkillRecord __instance, Pawn ___pawn, float xp, bool direct = false)
        {
            if (!ModSettings.spDecayLevelIncrease || direct) return;
            if (___pawn?.Faction != Faction.OfPlayer
                || xp > 0 
                || (xp < 0f && __instance.levelInt == 0)
                || __instance.XpRequiredForLevelUp - Math.Abs(xp) <= 0) return;

            xp /= __instance.LearnRateFactor();

            if (__instance.levelInt > 15) __instance.Learn(Math.Abs(xp / 2), true);
            else __instance.Learn(Math.Abs(xp), true);
        }
    }
}