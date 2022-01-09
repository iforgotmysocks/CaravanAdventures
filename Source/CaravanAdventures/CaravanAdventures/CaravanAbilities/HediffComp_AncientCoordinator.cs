using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientCoordinator : HediffComp
    {
        private int ticks = 0;
        private List<Pawn> linkedPawns = new List<Pawn>();
        private int ticksCheckHediffPresent = 0;
        private bool dialogAccepted = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Collections.Look(ref linkedPawns, "linkedPawns", LookMode.Reference);
            Scribe_Values.Look(ref dialogAccepted, "dialogNotAccepted", false);
        }

        public HediffComp_AncientCoordinator()
        {

        }

        private bool IsGifted() => Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientGift) != null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            var window = Dialog_MessageBox.CreateConfirmation("CAAbilityCoordinatorApprovalText".Translate(), delegate ()
            {
                var unlinkedHediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAura);
                if (unlinkedHediff != null) Pawn.health.RemoveHediff(unlinkedHediff);

                var hediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
                if (hediff != null) return;

                AddLinkedHediffToPawn(Pawn, Pawn);
                dialogAccepted = true;
            }, false, null);
            Find.WindowStack.Add(window);

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
            if (!dialogAccepted)
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }

            if (ticks > 180)
            {
                if (!IsGifted())
                {
                    RemoveAllLinkedAuras();
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
                var pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(pawn => !pawn.HasExtraHomeFaction()
                    && !pawn.HasExtraMiniFaction()
                    && pawn != Pawn
                    && !pawn.IsKidnapped()
                    && !CompatibilityDefOf.CACompatDef.raceDefsToExcludeFromAncientCoordinator.Contains(pawn.def.defName)
                    && !CompatibilityDefOf.CACompatDef.racesWithModExtsToExcludeFromAncientCoordinator
                    .Any(x => pawn?.def?.modExtensions != null && pawn.def.modExtensions
                        .Where(modExt => modExt != null)
                        .Select(modext => modext
                            .GetType()
                            .ToString())
                        .ToList()
                        .Contains(x))
                    ).ToList();
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
            if (pawns.Count <= ModSettings.maxLinkedAuraPawns) return;
            var pawnToKill = pawns.FirstOrDefault(pawn => !linkedPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked) && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift));
            if (pawnToKill == null) return;
            pawnToKill.Kill(null, parent);
        }

        private bool TryAddPawn(List<Pawn> pawns)
        {
            RemovePawnsNoLongerApplying(pawns);
            if (linkedPawns.Count >= pawns.Count || linkedPawns.Count >= ModSettings.maxLinkedAuraPawns) return false;
            var pawnToAdd = pawns.FirstOrDefault(pawn => !linkedPawns.Contains(pawn)
                && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientProtectiveAuraLinked)
                && !pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift
                ));
            if (pawnToAdd == null) return false;
            DLog.Message($"Adding {pawnToAdd} as linked aura pawn");
            AddLinkedHediffToPawn(pawnToAdd, Pawn);
            linkedPawns.Add(pawnToAdd);
            return true;
        }

        private void RemovePawnsNoLongerApplying(List<Pawn> pawns)
        {
            if (linkedPawns.Count < pawns.Count) return;
            var pawnsToRemove = linkedPawns.Where(p =>
                p == null
                || p.Dead
                || p.HasExtraHomeFaction()
                || p.HasExtraMiniFaction()
                || p.IsKidnapped()
                || CompatibilityDefOf.CACompatDef.raceDefsToExcludeFromAncientCoordinator.Contains(p.def.defName)
                || CompatibilityDefOf.CACompatDef.racesWithModExtsToExcludeFromAncientCoordinator
                    .Any(x => p?.def?.modExtensions != null && p.def.modExtensions
                        .Where(modExt => modExt != null)
                        .Select(modext => modext
                            .GetType()
                            .ToString())
                        .ToList()
                        .Contains(x))
                ).ToList();
            pawnsToRemove.ForEach(p => RemoveLinkedAura(p));
            linkedPawns.RemoveAll(p => pawnsToRemove.Contains(p));
            if (pawnsToRemove.Count > 0) DLog.Message($"Removing {pawnsToRemove.Count} linked aura pawns");
        }

        private void RemoveAllLinkedAuras()
        {
            RemoveLinkedAura(Pawn);

            foreach (var pawn in linkedPawns)
            {
                if (pawn == null) continue;
                RemoveLinkedAura(pawn);
            }
        }

        private void RemoveLinkedAura(Pawn pawn)
        {
            if (pawn == null) return;
            var hediff = pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
            if (hediff != null) pawn.health.RemoveHediff(hediff);
        }
    }
}
