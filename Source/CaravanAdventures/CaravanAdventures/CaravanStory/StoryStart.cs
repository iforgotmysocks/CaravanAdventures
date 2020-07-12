using System;
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
                //CheckEnsureGiftedAndAssignAbilities();

                AddTreeHumming();
                AddCompToTree();
                CheckPlayerProximityToInitiateDialog();
                
            }
        }

        private void AddCompToTree()
        {
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }

            // todo 
            //var cpTalk = new CompProperties_Talk() { compClass = typeof(CompTalk) };
            //var cTalk = new CompTalk();
            //cTalk.SetProps(cpTalk);
            //if (!tree.def.comps.Contains(cpTalk)) tree.def.comps.Add(cpTalk);
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

            var abilityDefs = DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("Ancient"));
            foreach (var abilityDef in abilityDefs)
            {
                chosen.abilities.GainAbility(abilityDef);
            }
            var lightAbilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "ConjureLight");
            chosen.abilities.GainAbility(lightAbilityDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false, false);
        }


    }
}
