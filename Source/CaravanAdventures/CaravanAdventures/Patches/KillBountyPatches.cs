﻿using System;
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
            if (CompCache.StoryWC == null || !ModSettings.storyEnabled || __instance.Faction != Faction.OfMechanoids) return;
            var instigator = dinfo.Value.Instigator as Pawn;
            if (instigator == null || instigator.Faction != Faction.OfPlayer) return;
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
            if (!ModSettings.storyEnabled || faction != CaravanStory.StoryUtility.FactionOfSacrilegHunters) return;

            var bountyNode = new DiaNode("CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints));
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { CompCache.StoryWC.BountyPoints += 100; bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, link = bountyNode }) ;
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyBack".Translate()) { link = __result });

            var optionBountyExchange = new DiaOption("CABountyExchangeOpenOption".Translate()) { link = bountyNode };

            __result.options.Insert(0, optionBountyExchange);
        }
    }
}
