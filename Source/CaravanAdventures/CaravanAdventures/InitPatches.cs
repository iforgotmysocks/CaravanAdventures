using System;
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

            // todo -> check for royalty
            PatchTreeDef_AddTalkOption();
            PatchHumanDef_AddTalkOption();
            PatchRemoveRoyalTitleRequirements();
            PatchAddPsychiteTeaToCampFire();
            PatchIncreaseBaseWealthAndFood();

            CompatibilityPatches.ExecuteCompatibilityPatches();
        }

        private static void PatchIncreaseBaseWealthAndFood()
        {
            var bases = DefDatabase<TraderKindDef>.AllDefsListForReading.Where(def => def.defName.ToLower().StartsWith("base_"));
            foreach (var curBase in bases)
            {
                DLog.Message($"adjusting base {curBase.defName}");
                var silverGen = curBase.stockGenerators.FirstOrDefault(gen => gen.HandlesThingDef(ThingDefOf.Silver));
                if (silverGen != null) silverGen.countRange = new IntRange(silverGen.countRange.min * 3, silverGen.countRange.max * 2);

                var foodGen = curBase.stockGenerators.FirstOrDefault(gen => gen.HandlesThingDef(ThingDefOf.MealSimple));
                if (foodGen != null) foodGen.countRange = new IntRange(foodGen.countRange.min * 2, Convert.ToInt32(foodGen.countRange.max * 1.5));
            }
        }

        private static void PatchRemoveRoyalTitleRequirements()
        {
            if (!ModsConfig.RoyaltyActive || !ModSettings.removeRoyalTitleRequirements) return;
            foreach (var def in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
            {
                if (ModSettings.removeOnlyAcolyteAndKnightRoyalTitleRequirements && !new[] { RoyalTitleDefOf.Knight, DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Esquire"), DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Acolyte") }.Contains(def)) continue;
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

        private static void PatchAddPsychiteTeaToCampFire()
        {
            var campFire = DefDatabase<ThingDef>.GetNamed(CaravanCamp.CampDefOf.CACampfireRoast.defName);
            if (campFire == null) return;
            var recipes = new[] { DefDatabase<RecipeDef>.GetNamed("Make_PsychiteTea"), DefDatabase<RecipeDef>.GetNamed("Make_PsychiteTeaBulk") };
            if (recipes.All(x => x == null)) return;
            foreach (var recipe in recipes) if (recipe != null) campFire.recipes.Add(recipe);
        }

    }
}
