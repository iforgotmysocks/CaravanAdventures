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
        private int ticks = 0;
        public TravelCompanionWC(World world) : base(world)
        {

        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (ticks > 1200)
            {
                ApplySocialRelations();
                //ApplySocialThoughts();
                ticks = 0;
            }
            ticks++;
        }

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

        private void ApplySocialRelations()
        {
            var playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.RaceProps.Humanlike).ToList();

            foreach (var mainPawn in playerPawns)
            {
                foreach (var pawn in playerPawns)
                {
                    if (pawn == mainPawn) continue;
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
            }
        }

        private TravelCompanionDef CalculateNewRelation(Pawn mainPawn, Pawn pawn)
        {
            var timeSpentTogether = Math.Abs(Find.TickManager.TicksGame - (Math.Abs(mainPawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal) - pawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal))));
            var companionDefs = DefDatabase<TravelCompanionDef>.AllDefs.OrderByDescending(y => y.thoughtStage).ToList();

            foreach (var def in companionDefs)
            {
                //Log.Message($"compareing time: {timeSpentTogether} with def {def.defName} maxDays: {def.maxDays} on pawns {mainPawn.Name} and {pawn.Name}");
                // todo add multiplier to adjust time based on how good or bad a pawn was treated
                // -> dive into log entries
                if (timeSpentTogether / 60000f >= def.maxDays) return def;
            }

            return null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }
    }
}
