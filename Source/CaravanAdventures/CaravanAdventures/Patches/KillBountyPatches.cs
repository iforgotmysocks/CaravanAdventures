using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace CaravanAdventures.Patches
{
    class KillBountyPatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            var pawnKillOrg = AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
            //var carTravelOrg = AccessTools.Method(typeof(Caravan), "get_NightResting");
            var pawnKillPost = new HarmonyMethod(typeof(KillBountyPatches).GetMethod(nameof(PawnKillPostfix)));
            harmony.Patch(pawnKillOrg, null, pawnKillPost);

            //var factionTryOpenCommsOrg = AccessTools.Method(typeof(Faction), nameof(Faction.TryOpenComms));
            ////var carTravelOrg = AccessTools.Method(typeof(Caravan), "get_NightResting");
            //var factionTryOpenCommsPost = new HarmonyMethod(typeof(CaravanTravel).GetMethod(nameof(PawnKillPostfix)));
            //harmony.Patch(factionTryOpenCommsOrg, null, factionTryOpenCommsPost);

            var factionDialogMakerFactionDialogForOrg = AccessTools.Method(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor));
            //var carTravelOrg = AccessTools.Method(typeof(Caravan), "get_NightResting");
            var factionDialogMakerFactionDialogForPost = new HarmonyMethod(typeof(KillBountyPatches).GetMethod(nameof(FactionDialogMakerFactionDialogForPostfix)));
            harmony.Patch(factionDialogMakerFactionDialogForOrg, null, factionDialogMakerFactionDialogForPost);
        }

        public static void PawnKillPostfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (CompCache.StoryWC == null || !ModSettings.storyEnabled || dinfo == null || CompCache.BountyWC?.BountyServiceAvailable != true || __instance?.Faction != Faction.OfMechanoids) return;
            var instigator = ModSettings.allowBountyFromBuildingInstigators ? dinfo.Value.Instigator : dinfo.Value.Instigator as Pawn;
            if (instigator == null || instigator?.Faction != Faction.OfPlayer) return;
            CaravanStory.StoryUtility.AddBountyPointsForKilledMech(__instance);
        }

        //public static void FactionTryOpenCommsPostfix(Faction __instance, Pawn negotiator)
        //{
        //    if (!ModSettings.storyEnabled || __instance != CaravanStory.StoryUtility.FactionOfSacrilegHunters) return;
        //    Find.WindowStack.TryRemove(typeof(Dialog_Negotiation), false);
            
        //    var dialog_Negotiation = new Dialog_Negotiation(negotiator, __instance, FactionDialogMaker.FactionDialogFor(negotiator, this), true);
        //    dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
        //    Find.WindowStack.Add(dialog_Negotiation);
        //}

        public static void FactionDialogMakerFactionDialogForPostfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            // todo adjust to non story and selectable faction!
            if (!ModSettings.storyEnabled || faction != CaravanStory.StoryUtility.FactionOfSacrilegHunters || CompCache.BountyWC?.BountyServiceAvailable != true) return;
            var request = new CaravanMechBounty.BountyRequest(__result, negotiator, faction);
            __result.options.Insert(0, request.CreateInitialDiaMenu());
        }
    }
}
