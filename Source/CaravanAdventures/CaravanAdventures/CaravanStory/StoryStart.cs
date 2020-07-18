﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory
{
    class StoryStart : MapComponent
    {
        private Dictionary<string, bool> storyFlags = new Dictionary<string, bool>()
        {
            { "Start_InitialTreeWhisper", false },
            { "Start_MapTreeWhisper", false },
            { "Start_RechedTree", false }
        };
        private Sustainer animaTreeWhipserSustainer;

        public StoryStart(Map map) : base(map)
        {
            
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
           
            //AddTalkTreeAction();
        }

        public override void MapGenerated()
        {
            base.MapGenerated();

            // ModLister.RoyaltyInstalled or ModsConfig.RoyaltyActive
            //CheckEnsureGiftedAndAssignAbilities();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                CheckEnsureGiftedAndAssignAbilities();


                //AddTreeHumming();
                //CheckPlayerProximityToInitiateDialog();
            }
        }

        private void AddTalkTreeAction()
        {
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return; 
            }

            // todo 
            Log.Message("adding tree action");
            var cp = tree.def.comps.FirstOrDefault(x => x is CompProperties_Talk) as CompProperties_Talk;
            cp.actions.Add(tree, (initiator, addressed) => StoryStartDialog(initiator, addressed));
            cp.enabled = true;
        }

        private void AddTreeHumming()
        {
            if (storyFlags["Start_InitialTreeWhisper"] == true) return;
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }
            
            var info = SoundInfo.InMap(tree, MaintenanceType.None);

            animaTreeWhipserSustainer = DefDatabase<SoundDef>.GetNamed("AnimaTreeWhispers").TrySpawnSustainer(info);
            if (animaTreeWhipserSustainer != null) storyFlags["Start_InitialTreeWhisper"] = true;
        }

        private void CheckPlayerProximityToInitiateDialog()
        {
        }

        private void CheckEnsureGiftedAndAssignAbilities()
        {
            // todo IsColonistPlayerControlled doesn't work here, when pawn is breaking it won't be playercontrolled anymore
            var pawns = PawnsFinder.AllMapsAndWorld_Alive.Where(x => x.RaceProps.Humanlike && x.Faction.IsPlayer);
            if (pawns == null || pawns?.Count() == 0)
            {
                Log.Message($"No pawns found, skipping.");
                return;
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

            var abilityDefs = DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("Ancient"));
            foreach (var abilityDef in abilityDefs)
            {
                chosen.abilities.GainAbility(abilityDef);
            }
            var lightAbilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "ConjureLight");
            chosen.abilities.GainAbility(lightAbilityDef);
        }

        public void StoryStartDialog(Pawn initiator, object addressed)
        {
            Log.Message($"Story starts initiated by {initiator.Name} and {((Thing)addressed).def.defName}");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false, false);
        }


    }
}
