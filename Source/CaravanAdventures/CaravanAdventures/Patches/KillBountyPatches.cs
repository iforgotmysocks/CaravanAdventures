using RimWorld;
using Verse;
using HarmonyLib;

namespace CaravanAdventures.Patches
{
    class KillBountyPatches
    {
        public static bool killBountyPatchesApplied = false;
        public static void ApplyPatches()
        {
            if (!ModSettings.bountyEnabled) return;
            var pawnKillOrg = AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
            var pawnKillPost = new HarmonyMethod(typeof(KillBountyPatches).GetMethod(nameof(PawnKillPostfix)));
            HarmonyPatcher.harmony.Patch(pawnKillOrg, null, pawnKillPost);

            var factionDialogMakerFactionDialogForOrg = AccessTools.Method(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor));
            var factionDialogMakerFactionDialogForPost = new HarmonyMethod(typeof(KillBountyPatches).GetMethod(nameof(FactionDialogMakerFactionDialogForPostfix)));
            HarmonyPatcher.harmony.Patch(factionDialogMakerFactionDialogForOrg, null, factionDialogMakerFactionDialogForPost);

            killBountyPatchesApplied = true;
        }

        public static void PawnKillPostfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            // todo may need to improve that faction check...
            if (!ModSettings.bountyEnabled 
                || CompCache.StoryWC == null 
                || dinfo == null 
                || CompCache.BountyWC?.BountyServiceAvailable != true 
                || __instance?.Faction?.def == null 
                || (__instance?.Faction != Helper.ExpRMNewFaction
                    && !CompatibilityDefOf.CACompatDef.additionalBountyFactionDefsToAdd.Contains(__instance?.Faction?.def?.defName))) return;
            var instigator = ModSettings.allowBountyFromBuildingInstigators ? dinfo.Value.Instigator : dinfo.Value.Instigator as Pawn;
            if (instigator == null || instigator?.Faction != Faction.OfPlayer) return;
            CaravanStory.StoryUtility.AddBountyPointsForKilledMech(__instance);
        }

        public static void FactionDialogMakerFactionDialogForPostfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            if (!ModSettings.bountyEnabled || CompCache.BountyWC?.BountyServiceAvailable != true || faction != CompCache.BountyWC.BountyFaction) return;
            var request = new CaravanMechBounty.BountyRequest(__result, negotiator, faction);
            __result.options.Insert(0, request.CreateInitialDiaMenu());
        }
    }
}
