using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientCoordinator : HediffComp
    {
        private int ticks = 0;
        private List<Pawn> linkedPawns = new List<Pawn>();

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Collections.Look(ref linkedPawns, "linkedPawns", LookMode.Reference);
        }

        public HediffComp_AncientCoordinator()
        {

        }

        private bool IsGifted() => Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientGift) != null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticks > 50)
            {
                if (!IsGifted())
                {
                    RemoveAllLinkedAuras();
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
                // todo move 4 to modSettings
                var pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                TryAddPawn(pawns);
                TryKillPawn(pawns);
                ticks = 0;
            }

        }

        private void TryKillPawn(List<Pawn> pawns)
        {
            if (pawns.Count <= linkedPawns.Count + 1) return;
            var pawnToKill = pawns.FirstOrDefault(pawn => !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift));
            if (pawnToKill == null) return;
            pawnToKill.Kill(null, parent);
        }

        private void TryAddPawn(List<Pawn> pawns)
        {
            if ((linkedPawns.Count + 1) >= pawns.Count || linkedPawns.Count >= ModSettings.maxLinkedAuraPawns) return;
            var pawnToAdd = pawns.FirstOrDefault(pawn => !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked));
            if (pawnToAdd == null) return;
            pawnToAdd.health.AddHediff(HediffMaker.MakeHediff(AbilityDefOf.CAAncientProtectiveAuraLinked, Pawn, Pawn.health.hediffSet.GetBrain()));
            linkedPawns.Add(pawnToAdd);
        }

        private void RemoveAllLinkedAuras()
        {
            foreach (var pawn in linkedPawns)
            {
                if (pawn == null) continue;
                var hediff = pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
