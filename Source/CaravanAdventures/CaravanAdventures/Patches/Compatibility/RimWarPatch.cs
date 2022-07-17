using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace CaravanAdventures.Patches.Compatibility
{
    public class RimWarPatch
    {
        private static Assembly assembly;
        public static void ApplyPatches(Assembly assembly)
        {
            if (!ModSettings.storyEnabled) return;
            RimWarPatch.assembly = assembly;
            var org = AccessTools.Method(assembly.GetType("RimWar.Planet.WorldComponent_PowerTracker"), "GetFactionForVictoryChallenge");
            var post = new HarmonyMethod(typeof(RimWarPatch).GetMethod(nameof(WorldComponent_PowerTracker_GetFactionForVictoryChallenge_Postfix)));
            HarmonyPatcher.harmony.Patch(org, null, post);
        }

        public static void WorldComponent_PowerTracker_GetFactionForVictoryChallenge_Postfix(object __instance)
        {
            var factionFieldInfo = assembly.GetType("RimWar.Planet.WorldComponent_PowerTracker").GetField("victoryFaction", BindingFlags.Instance | BindingFlags.Public);
            var faction = factionFieldInfo.GetValue(__instance) as Faction;

            if (faction != CaravanStory.StoryUtility.FactionOfSacrilegHunters) return;

            var methodInfoVassal = assembly.GetType("RimWar.Planet.WorldUtility").GetMethod("IsVassalFaction", BindingFlags.Static | BindingFlags.Public);
            var newHostileFaction = Find.FactionManager.AllFactions.FirstOrDefault(x => !x?.def?.hidden == true && x?.def?.humanlikeFaction == true && x != Faction.OfPlayer && !(bool)methodInfoVassal.Invoke(null, new object[] { x }));

            if (newHostileFaction == null)
            {
                Log.Error($"Switching the current hostile victory faction from sacrileg hunters to a different random faction failed. Please enable devmode to set another faction to Rival / the current victory Faction, as sacrileg hunters aren't compatible as a rival faction.");
                return;
            }

            factionFieldInfo.SetValue(__instance, newHostileFaction);
            Log.Message($"Updated the current rimwar victory / rival faction from sacrileg hunters to {newHostileFaction}.");
            var existingLetter = Find.LetterStack.LettersListForReading.LastOrDefault();
            if (existingLetter != null) Find.LetterStack.RemoveLetter(existingLetter);
            Find.LetterStack.ReceiveLetter("RW_VictoryChallengeLabel".Translate(), "RW_VictoryChallengeMessage".Translate(newHostileFaction.Name), LetterDefOf.ThreatBig, null);
        }
    }
}