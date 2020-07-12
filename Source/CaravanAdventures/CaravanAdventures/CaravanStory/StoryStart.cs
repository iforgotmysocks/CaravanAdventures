using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CaravanAdventures.CaravanStory
{
    class StoryStart : MapComponent
    {
        private bool chosenPawnSelected = false;

        public StoryStart(Map map) : base(map)
        {

        }

        public override void MapGenerated()
        {
            base.MapGenerated();

            // ModLister.RoyaltyInstalled or ModsConfig.RoyaltyActive

            CheckEnsureGiftedAndAssignAbilities();
            if (chosenPawnSelected) Log.Message("yup, chosen was loaded");
            chosenPawnSelected = true;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 60000 == 0) CheckEnsureGiftedAndAssignAbilities();
        }

        private void CheckEnsureGiftedAndAssignAbilities()
        {
            // todo IsColonistPlayerControlled doesn't work here, when pawn is breaking it won't be playercontrolled anymore
            var pawns = PawnsFinder.AllMapsAndWorld_Alive.Where(x => x.RaceProps.Humanlike && x?.Faction?.def == FactionDefOf.PlayerColony);
            if (pawns == null || pawns?.Count() == 0)
            {
                Log.Message($"No pawns found, skipping.");
                return;
            }
            foreach (var pawn in pawns)
            {
                Log.Message($"Pawns found: {pawn?.Name} {pawn?.Faction?.Name}");
            }

            if (pawns.FirstOrDefault(x => x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null) != null)
            {
                Log.Message($"A gifted char already exists, skipping");
                return;
            }

            // todo make sure the pawn isn't psychially unsensitive.
            var chosen = pawns.FirstOrDefault(x => x.Name.ToString().ToLower().Contains("kay")) ?? pawns.FirstOrDefault();
            if (chosen == null)
            {
                Log.Message("Couldn't find pawn, skipping");
                return;
            }

            Log.Message(chosen.Name + " " + chosen.NameFullColored);

            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), chosen.health.hediffSet.GetBrain());
            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientGift"), chosen.health.hediffSet.GetBrain());

            var abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientMeditate");
            chosen.abilities.GainAbility(abilityDef);
            abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientProtectiveAura");
            chosen.abilities.GainAbility(abilityDef);
            abilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "ConjureLight");
            chosen.abilities.GainAbility(abilityDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false, false);
        }


    }
}
