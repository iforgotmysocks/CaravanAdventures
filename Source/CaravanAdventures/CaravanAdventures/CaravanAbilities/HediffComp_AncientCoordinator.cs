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
        private int ticksCheckHediffPresent = 0;

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

            var unlinkedHediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAura);
            if (unlinkedHediff != null) Pawn.health.RemoveHediff(unlinkedHediff);

            Log.Message($"trying to add to gifted pawn");
            var hediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
            if (hediff != null) return;

            AddLinkedHediffToPawn(Pawn, Pawn);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            RemoveAllLinkedAuras();
        }

        private void AddLinkedHediffToPawn(Pawn pawn, Pawn connector)
        {
            var createdHediff = HediffMaker.MakeHediff(AbilityDefOf.CAAncientProtectiveAuraLinked, pawn);
            var linkedComp = createdHediff.TryGetComp<HediffComp_AncientProtectiveAura>();
            if (linkedComp == null) return;
            linkedComp.Connector = connector;
            pawn.health.AddHediff(createdHediff, pawn.health.hediffSet.GetBrain());
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn == null || Pawn.Dead) return;

            if (ticks > 50)
            {
                if (!IsGifted())
                {
                    RemoveAllLinkedAuras();
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
                var pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                if (!TryAddPawn(pawns)) TryKillPawn(pawns);
                ticks = 0;
            }

            if (ticksCheckHediffPresent > 300)
            {
                EnsureHediffPresent();
                ticksCheckHediffPresent = 0;
            }

            ticks++;
            ticksCheckHediffPresent++;
        }

        private void EnsureHediffPresent()
        {
            var hediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
            if (hediff == null) AddLinkedHediffToPawn(Pawn, Pawn);

            foreach (var pawn in linkedPawns)
            {
                if (pawn == null) continue;
                hediff = pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
                if (hediff == null) AddLinkedHediffToPawn(pawn, Pawn);
            }
        }

        private void TryKillPawn(List<Pawn> pawns)
        {
            if (pawns.Count <= (ModSettings.maxLinkedAuraPawns + 1)) return;
            var pawnToKill = pawns.FirstOrDefault(pawn => !linkedPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift));
            if (pawnToKill == null) return;
            pawnToKill.Kill(null, parent);
        }

        private bool TryAddPawn(List<Pawn> pawns)
        {
            if ((linkedPawns.Count + 1) >= pawns.Count || linkedPawns.Count >= ModSettings.maxLinkedAuraPawns) return false;
            var pawnToAdd = pawns.FirstOrDefault(pawn => !linkedPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift));
            if (pawnToAdd == null) return false;
            Log.Message($"trying to add {pawnToAdd}");
            AddLinkedHediffToPawn(pawnToAdd, Pawn);
            linkedPawns.Add(pawnToAdd);
            return true;
        }

        private void RemoveAllLinkedAuras()
        {
            var hediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
            if (hediff != null) Pawn.health.RemoveHediff(hediff);

            foreach (var pawn in linkedPawns)
            {
                if (pawn == null) continue;
                hediff = pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
