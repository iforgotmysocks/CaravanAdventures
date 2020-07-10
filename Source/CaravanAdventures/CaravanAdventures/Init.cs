﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using CaravanAdventures.CaravanItemSelection;

namespace CaravanAdventures
{
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
            PatchHediffsWhenEnabled();
        }

        private void PatchHediffsWhenEnabled()
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
                option.chance = 0.8;

                // todo add new option cat to add another group of special tiems 

                var newOption = option;
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
