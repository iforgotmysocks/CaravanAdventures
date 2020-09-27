using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using CaravanAdventures.CaravanItemSelection;
using CaravanAdventures.CaravanStory;

namespace CaravanAdventures
{
    // todo https://fluffy-mods.github.io//2020/08/13/debugging-rimworld/
    // todo GameComponent??? -> compprops applied to defs within a gamecomp somehow lead to missing comps on the object of the def which doesn't happen with world comps
    class Init : WorldComponent
    {
        private int removeRuinsTick = 0;

        public Init(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FilterCombs.InitFilterSets();
            PatchAncientShrineDefs_MoreShrinesAndBetterRewards();
            PatchTreeDef_AddTalkOption();
            PatchHumanDef_AddTalkOption();
            PatchRemovePenaltyForBeingRoyal();
        }

        private void PatchRemovePenaltyForBeingRoyal()
        {
            var defNames = new string[] { "TitleApparelRequirementNotMet", "TitleApparelMinQualityNotMet", "TitleNoThroneRoom", "TitleNoPersonalBedroom", "TitleThroneroomRequirementsNotMet", "TitleBedroomRequirementsNotMet"};
            foreach (var def in DefDatabase<ThoughtDef>.AllDefsListForReading.Where(x => defNames.Contains(x.defName)))
            {
                def.stages.First().baseMoodEffect = 0;
            }
        }

        private void PatchTreeDef_AddTalkOption()
        {
            var tree = DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima");
            
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }
            var compProp = new CompProperties_Talk();
            if (!tree.comps.Any(x => x is CompProperties_Talk)) tree.comps.Add(compProp);
        }

        private void PatchHumanDef_AddTalkOption()
        {
            var humanDef = DefDatabase<ThingDef>.GetNamed("Human");

            if (humanDef == null)
            {
                Log.Message("HumanDef is null");
                return;
            }
            // todo reenable?
            var compProp = new CompProperties_Talk();
            if (!humanDef.comps.Any(x => x is CompProperties_Talk)) humanDef.comps.Add(compProp);
        }

        private void PatchAncientShrineDefs_MoreShrinesAndBetterRewards()
        {
            // todo ModOptions
            var scatterShrinesDef = DefDatabase<GenStepDef>.GetNamed("ScatterShrines");
            var genStep = scatterShrinesDef.genStep as GenStep_ScatterShrines;
            if (genStep != null) 
            {
                genStep.countPer10kCellsRange.min *= 2;
                genStep.countPer10kCellsRange.max *= 2;
            }

            var templeContentsDef = DefDatabase<ThingSetMakerDef>.GetNamed("MapGen_AncientTempleContents");
            var root = templeContentsDef.root as ThingSetMaker_Sum;
            if (root != null) 
            {
                var option = root.options[1];
                option.chance = 0.8f;

                // todo add new option cat to add another group of special tiems 

                var newOption = option;
                var thingSetMaker_Count = newOption.thingSetMaker as ThingSetMaker_Count;
                // maybe reserve this for special rewards?
                thingSetMaker_Count.fixedParams.filter.Allows(ThingDefOf.VanometricPowerCell);
                thingSetMaker_Count.fixedParams.filter.Allows(ThingDefOf.InfiniteChemreactor);
                root.options.Add(newOption);


                //var newItems = new ThingDef[]
                //{
                //    DefDatabase<ThingDef>.GetNamed("MechSerumHealer"),
                //    DefDatabase<ThingDef>.GetNamed("MechSerumResurrector"),
                //};

                //var newFilter = new ThingFilter();
                //foreach (var item in newItems)
                //{
                //    newFilter.Allows(item);
                //}

                //root.options.Add(new ThingSetMaker_Sum.Option
                //{
                //    chance = 1,
                //    thingSetMaker = new ThingSetMaker_Count()
                //    {
                //        fixedParams = new ThingSetMakerParams()
                //        {
                //            filter = newFilter,
                //        }
                //    }
                //});
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            RemoveRuins();
            removeRuinsTick++;
        }

        private void RemoveRuins()
        {
            if (removeRuinsTick > 60000)
            {
                var settlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement.def == WorldObjectDefOf.AbandonedSettlement && settlement.Faction.IsPlayer);
                Log.Message($"Trying to remove {settlements.Count()} settlements");

                foreach (var settlement in settlements.Reverse())
                {
                    Find.WorldObjects.Remove(settlement);
                }
                removeRuinsTick = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removeRuinsTick, "removeRuinsTick", 0);
        }


    }
}
