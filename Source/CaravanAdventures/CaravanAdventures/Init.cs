using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CaravanAdventures
{
    class Init : MapComponent
    {
        private bool chosenPawnSelected;

        public Init(Map map) : base(map)
        {
            // todo figure out why chosenPawnSelected isn't being saved!
            if (!chosenPawnSelected)
            {
                //var pawns = PawnsFinder.AllMapsWorldAndTemporary_Alive.Where(x => x.Faction.def == FactionDefOf.PlayerColony);
                
                //var chosen = pawns.FirstOrDefault(x => x.Name.ToString().ToLower().Contains("kay")) ?? pawns.FirstOrDefault();
                //if (chosen == null)
                //{
                //    Log.Message("Couldn't find pawn, skipping");
                //    return;
                //}

                //Log.Message(chosen.Name + " " + chosen.NameFullColored);

                //chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), chosen.health.hediffSet.GetBrain());
                //chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientGift"), chosen.health.hediffSet.GetBrain());

                //var abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientMeditate");
                //chosen.abilities.GainAbility(abilityDef);
                //abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientProtectiveAura");
                //chosen.abilities.GainAbility(abilityDef);
                //abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "ConjureLight");
                //chosen.abilities.GainAbility(abilityDef);
                //chosenPawnSelected = true;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false);
        }


    }
}
