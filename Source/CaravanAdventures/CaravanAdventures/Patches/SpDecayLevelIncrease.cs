﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class SpDecayLevelIncrease
    {
        public static void ApplyPatches(Harmony harmony)
        {
            var skillOrg = AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Learn));
            var skillPost = new HarmonyMethod(typeof(SpDecayLevelIncrease).GetMethod(nameof(Learn_Postfix)));
            harmony.Patch(skillOrg, null, skillPost);
        }

        public static void Learn_Postfix(SkillRecord __instance, Pawn ___pawn, float xp, bool direct = false)
        {
            if (!ModSettings.spDecayLevelIncrease || direct) return;
            if (___pawn?.Faction != Faction.OfPlayer
                || xp > 0 
                || (xp < 0f && __instance.levelInt == 0)
                || __instance.XpRequiredForLevelUp - Math.Abs(xp) <= 0
                || __instance.levelInt > 15) return;
            
            __instance.Learn(Math.Abs(xp), true);
        }
    }
}