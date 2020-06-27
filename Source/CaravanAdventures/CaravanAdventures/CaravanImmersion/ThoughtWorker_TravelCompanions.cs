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
        //private List<TravelCompanionDef> companionDefs;
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            TravelCompanionDef companionDef = GetCurrentDef(p);
            if (companionDef == null)
            {
                return ThoughtState.Inactive;
            }
            //var adjustedThoughtStage = ApplySocialBonds(p, companionDef);

            return ThoughtState.ActiveAtStage(companionDef.thoughtStage);
        }

        private TravelCompanionDef ApplySocialBonds(Pawn p, TravelCompanionDef companionDef)
        {
            // todo
            return companionDef;
        }

        public static TravelCompanionDef GetCurrentDef(Pawn p)
        {
            if (Current.ProgramState != ProgramState.Playing) return null;
            if (p.Faction != Faction.OfPlayer) return null;

            var companionDefs = DefDatabase<TravelCompanionDef>.AllDefs.OrderByDescending(y => y.thoughtStage).ToList();

            var time = p.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal);
            foreach (var def in companionDefs)
            {
                if (time / 60000f > def.maxDays) return def;
            }

            return null;
        }

    }
}
