using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    public class TravelCompanionWC : WorldComponent
    {
        private int ticks = 1000;
        private List<PawnRelationDef> relationShipsWithImpact;
        private bool wasActive = false;

        public TravelCompanionWC(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref wasActive, "wasActive", false);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            relationShipsWithImpact = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where(x => !x.HasModExtension<TravelCompanionModExt>() && (x.familyByBloodRelation || (x.reflexive && !x.defName.StartsWith("Ex")))).ToList();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (!ModSettings.caravanTravelCompanionsEnabled)
            {
                if (wasActive)
                {
                    wasActive = false;
                    RemoveRelations();
                }
                return;
            }
            wasActive = true;
            ApplySocialRelations();
            ticks++;
        }

        private void RemoveRelations()
        {
            var playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.RaceProps.Humanlike).ToList();

            foreach (var mainPawn in playerPawns)
            {
                var pawnRelations = mainPawn.relations.DirectRelations;
                foreach (var relation in pawnRelations.Reverse<DirectPawnRelation>())
                {
                    if (relation.def.GetModExtension<TravelCompanionModExt>() != null) mainPawn.relations.RemoveDirectRelation(relation.def, relation.otherPawn);
                }
            }
        }

        private void ApplySocialRelations()
        {
            if (ticks > 1200)
            {
                var playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.RaceProps.Humanlike).ToList();
                playerPawns.RemoveAll(x =>
                {
                    if (CompatibilityDefOf.CACompatDef.raceDefsToExcludeFromTravelCompanions.Contains(x.def.defName)) return true;
                    foreach (var modExtsToCheck in CompatibilityDefOf.CACompatDef.racesWithModExtsToExcludeFromTravelCompanions)
                    {
                        if (x.def.modExtensions != null 
                            && x.def.modExtensions.Any(modExt => modExt?.GetType()?.ToString() == modExtsToCheck)) 
                                return true;
                    }
                    return false;
                });

                foreach (var mainPawn in playerPawns)
                {
                    if (mainPawn.IsBorrowedByAnyFaction() || mainPawn.Dead || mainPawn.HasExtraMiniFaction() || mainPawn.HasExtraHomeFaction()) continue;
                    foreach (var pawn in playerPawns)
                    {
                        if (pawn == mainPawn || pawn.IsBorrowedByAnyFaction() || pawn.Dead || pawn.HasExtraMiniFaction() || pawn.HasExtraHomeFaction()) continue;
                        var currentRelation = pawn.relations.DirectRelations.FirstOrDefault(x => (x.def.GetModExtension<TravelCompanionModExt>()?.isTravelCompanionRelation ?? false) == true && x.otherPawn == mainPawn);
                        var newRelation = CalculateNewRelation(mainPawn, pawn);
                        if (newRelation == null)
                        {
                            Log.Error($"newRelation is null, which should not be happening!!! -> TravelCompanionWC -> ApplySocialRelation -> CalculateNewRelation()");
                            continue;
                        }
                        if (currentRelation != null && currentRelation.def.defName == newRelation.relationDefName) continue;
                        if (currentRelation != null && currentRelation.def.defName != newRelation.relationDefName)
                        {
                            pawn.relations.RemoveDirectRelation(currentRelation.def, mainPawn);
                        }
                        pawn.relations.AddDirectRelation(TravelCompanionDefOf.RelationNamed(newRelation.relationDefName), mainPawn);
                    }

                    foreach (var relation in mainPawn.relations.DirectRelations.Reverse<DirectPawnRelation>())
                    {
                        if (relation?.otherPawn?.Dead != true) continue;
                        if (relation.def.GetModExtension<TravelCompanionModExt>() != null) mainPawn.relations.RemoveDirectRelation(relation.def, relation.otherPawn);
                    }
                }

                ticks = 0;
            }
        }

        private TravelCompanionDef CalculateNewRelation(Pawn mainPawn, Pawn pawn)
        {
            var timeSpentTogether = Math.Min(mainPawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal), pawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal));
            var companionDefs = DefDatabase<TravelCompanionDef>.AllDefs.OrderByDescending(y => y.thoughtStage).ToList();

            if (mainPawn.relations.DirectRelations.Any(x => relationShipsWithImpact.Contains(x.def) && x.otherPawn == pawn) || BothPawnsStartPawns(mainPawn, pawn)) timeSpentTogether += 60000f * 60;

            //DLog.Message($"{mainPawn.Name} {pawn.Name} : {Math.Round(timeSpentTogether / 60000f)} days, startpawns?: {BothPawnsStartPawns(mainPawn, pawn)} relationship? {mainPawn.relations.DirectRelations.Any(x => relationShipsWithImpact.Contains(x.def) && x.otherPawn == pawn)} borrowed: {mainPawn.IsBorrowedByAnyFaction()} otherhomefaction: {mainPawn.HasExtraHomeFaction()}");

            foreach (var def in companionDefs)
            {
                //DLog.Message($"compareing time: {Math.Round(timeSpentTogether / 60000, 0)} with def {def.defName} maxDays: {def.maxDays} on pawns {mainPawn.Name} and {pawn.Name}");
                // todo add multiplier to adjust time based on how good or bad a pawn was treated
                // -> dive into log entries
                if (timeSpentTogether / 60000f >= def.maxDays) return def;
            }

            return null;
        }

        private bool BothPawnsStartPawns(Pawn mainPawn, Pawn pawn)
        {
            if ((Find.TickManager.TicksGame - mainPawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal)) < 5000f && (Find.TickManager.TicksGame - pawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal)) < 5000f) return true;
            return false;
        }


        #region shelved stuff
        // currently shelved
        private void ApplySocialThoughts()
        {
            var playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.RaceProps.Humanlike).ToList();

            foreach (var mainPawn in playerPawns)
            {
                foreach (var pawn in playerPawns)
                {
                    if (pawn == mainPawn) continue;
                    var memory = pawn.needs.mood.thoughts.memories.Memories.FirstOrDefault(x => x.otherPawn == mainPawn && x.def.HasModExtension<TravelCompanionModExt>());
                    //.DirectRelations.FirstOrDefault(x => (x.def.GetModExtension<TravelCompanionModExt>()?.isTravelCompanionRelation ?? false) == true && x.otherPawn == mainPawn);
                    var newRelation = CalculateNewRelation(mainPawn, pawn);
                    if (newRelation == null)
                    {
                        Log.Error($"newRelation is null, which should not be happening!!! -> TravelCompanionWC -> ApplySocialRelation -> CalculateNewRelation()");
                        continue;
                    }
                    if (memory != null && memory.def.defName == newRelation.relationDefName) continue;
                    if (memory != null && memory.def.defName != newRelation.relationDefName)
                    {
                        pawn.needs.mood.thoughts.memories.Memories.Remove(memory);
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(TravelCompanionDefOf.ThoughtNamed(newRelation.relationDefName), mainPawn);
                }
            }
        }

        #endregion

    }
}
