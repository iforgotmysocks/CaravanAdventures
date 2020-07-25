using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanImmersion
{
    class ThoughtWorker_TravelCompanions : ThoughtWorker
    {
        private List<TravelCompanionDef> companionDefs;
        public ThoughtWorker_TravelCompanions()
        {
            companionDefs = DefDatabase<TravelCompanionDef>.AllDefs.OrderByDescending(y => y.thoughtStage).ToList();
        }
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            TravelCompanionDef companionDef = GetCurrentDef(p);
            if (companionDef == null)
            {
                return ThoughtState.Inactive;
            }

            return ThoughtState.ActiveAtStage(companionDef.thoughtStage);
        }

        public TravelCompanionDef GetCurrentDef(Pawn p)
        {
            if (Current.ProgramState != ProgramState.Playing) return null;
            if (p.Faction != Faction.OfPlayer || p.IsBorrowedByAnyFaction() || p.HasExtraMiniFaction() || p.HasExtraHomeFaction()) return null;

            var time = p.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal);
            if (Find.TickManager.TicksGame - time < 5000) time += 60000f * 60;
            foreach (var def in companionDefs)
            {
                if (time / 60000f > def.maxDays) return def;
            }

            return null;
        }

    }
}
