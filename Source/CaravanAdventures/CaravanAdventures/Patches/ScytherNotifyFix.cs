using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class ScytherNotifyFix
    {
        public static void ApplyPatches()
        {
            var org = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.ShouldSendNotificationAbout));
            var post = new HarmonyMethod(typeof(ScytherNotifyFix).GetMethod(nameof(ShouldSendNotificationAboutPostfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static void ShouldSendNotificationAboutPostfix(ref bool __result, Pawn p)
        {
            if (p?.RaceProps?.IsMechanoid == false || p?.Faction != Faction.OfPlayer || p?.def?.defName != "Mech_Scyther") return;
            var hediff = p?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x?.def?.defName == "CAOverheatingBrain");
            if (hediff == null) return;
            __result = false;
        }
    }
}