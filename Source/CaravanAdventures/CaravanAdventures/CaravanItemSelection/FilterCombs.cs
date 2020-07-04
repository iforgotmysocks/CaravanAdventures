using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaravanAdventures.CaravanItemSelection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using CaravanAdventures.Patches;

namespace CaravanAdventures.CaravanItemSelection
{
    class FilterCombs
    {
        public static FilterSet packUp;
        public static FilterSet goods;
        public static FilterSet journey;

        public static void InitFilterSets()
        {
            packUp = new FilterSet();
            goods = new FilterSet();
            journey = new FilterSet();

            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDefOf.Chunks,
                ThingCategoryDefOf.StoneChunks })
            {
                Name = "Chunks",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDefOf.Corpses,
                ThingCategoryDefOf.CorpsesAnimal,
                ThingCategoryDefOf.CorpsesHumanlike,
                ThingCategoryDefOf.CorpsesInsect,
                ThingCategoryDefOf.CorpsesMechanoid })
            {
                Name = "Corpses",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDefOf.ResourcesRaw,
                ThingCategoryDefOf.StoneBlocks,
                ThingCategoryDefOf.Buildings })
            {
                Name = "BuildMaterials",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingDefOf.Gold,
                ThingDefOf.Silver,
                ThingDefOf.Plasteel})
            {
                Name = "IncludeBuildMaterials",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Include,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                DefDatabase<ThingDef>.GetNamed("UnfinishedSculpture") })
            {
                Name = "UnfinishedHeavyThings",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });

            goods.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDefOf.Apparel,
                DefDatabase<ThingCategoryDef>.GetNamed("WeaponsRanged"),
                DefDatabase<ThingCategoryDef>.GetNamed("WeaponsMelee"),
                ThingCategoryDefOf.BuildingsArt,
                DefDatabase<ThingCategoryDef>.GetNamed("Headgear"),
                DefDatabase<ThingCategoryDef>.GetNamed("ApparelNoble"),
                })
            {
                Name = "SellableGoods",
                Connection = FilterConnection.NEG,
                Operation = FilterOperation.Include,
                MaxQuality = QualityCategory.Excellent,
            });

            // fix NEG being used in a different context as AND / OR
            journey.appliedFilters.Add(new Filter(new object[]
            {
                ThingDefOf.MedicineHerbal,
                ThingDefOf.MedicineIndustrial })
            {
                Name = "Meds",
                Amount = 1,
                Connection = FilterConnection.NEG,
                Operation = FilterOperation.Include,
            });

            journey.appliedFilters.Add(new Filter(new object[]
            {
                ThingDefOf.Bedroll})
            {
                Name = "Bedroll",
                Amount = 1,
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Include,
            });

            journey.appliedFilters.Add(new Filter(new object[]
            {
                ThingDefOf.MealSurvivalPack,
                ThingDefOf.MealSimple,
                })
            {
                Name = "Bedroll",
                Amount = 5,
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Include,
            });


        }

        public static void ApplyAll(List<Patches.Section> sections)
        {
            foreach (var section in sections)
                foreach (var trans in section.cachedTransferables)
                    FilterHelper.SetMaxAmount(trans);
        }

        public static void ApplyPackUp(List<Patches.Section> sections)
        {
            foreach (var section in sections)
            {
                foreach (var trans in section.cachedTransferables)
                {
                    if (FilterHelper.DoFiltersApply(packUp, trans)) FilterHelper.SetMaxAmount(trans);
                    else FilterHelper.SetMinAmount(trans);
                }
            }
        }

        public static void ApplyJourney(List<Patches.Section> sections)
        {
            foreach (var section in sections)
            {
                foreach (var trans in section.cachedTransferables)
                {
                    // create new method just for this application type
                    //if (FilterHelper.DoFiltersApply(journey, trans)) FilterHelper.SetAmount(trans, amount);
                    //else FilterHelper.SetMinAmount(trans);
                }
            }
        }

        public static void ApplyGoods(List<Patches.Section> sections)
        {
            foreach (var section in sections)
            {
                foreach (var trans in section.cachedTransferables)
                {
                    if (FilterHelper.DoFiltersApply(goods, trans)) FilterHelper.SetMaxAmount(trans);
                    else FilterHelper.SetMinAmount(trans);
                }
            }
        }

        public static void ApplyNone(List<Patches.Section> sections)
        {
            foreach (var section in sections)
                foreach (var trans in section.cachedTransferables)
                    FilterHelper.SetMinAmount(trans);
        }
    }
}
