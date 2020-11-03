﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaravanAdventures.CaravanItemSelection;
using CaravanAdventures.CaravanStory;
using RimWorld;
using Verse;

namespace CaravanAdventures
{
    [StaticConstructorOnStartup]
    static class InitPatches
    {
        static InitPatches()
        {
            FilterCombs.InitFilterSets();
            PatchAncientShrineDefs_MoreShrinesAndBetterRewards();
            PatchTreeDef_AddTalkOption();
            PatchHumanDef_AddTalkOption();
            PatchRemoveRoyalTitleRequirements();
        }

        private static void PatchRemoveRoyalTitleRequirements()
        {
            foreach (var def in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
            {
                def.disabledJoyKinds = new List<JoyKindDef>();
                def.disabledWorkTags = WorkTags.None;
                def.requiredApparel = new List<RoyalTitleDef.ApparelRequirement>();
                def.bedroomRequirements = new List<RoomRequirement>();
                def.foodRequirement = default;
                def.throneRoomRequirements = new List<RoomRequirement>();
                def.requiredMinimumApparelQuality = QualityCategory.Awful;
            }
        }

        private static void PatchTreeDef_AddTalkOption()
        {
            var tree = DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima");

            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }
            if (!tree.comps.Any(x => x is CompProperties_Talk)) tree.comps.Add(new CompProperties_Talk());
        }

        private static void PatchHumanDef_AddTalkOption()
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

        private static void PatchAncientShrineDefs_MoreShrinesAndBetterRewards()
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
    }
}
