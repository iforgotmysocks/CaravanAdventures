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
                ThingCategoryDefOf.Buildings,
                DefDatabase<ThingCategoryDef>.GetNamed("BuildingsFurniture")})
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
            goods.appliedFilters.Add(new Filter(new object[]
            {
                DefDatabase<ThingCategoryDef>.GetNamedSilentFail("ApparelUtility")
            })
            {
                Name = "ExcludedSellableGoods",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude
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
                Name = "Meals",
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

        public static void ApplyJourney(List<Patches.Section> sections, List<Pawn> caravanMembers)
        {
            FilterHelper.ApplyTravelSupplies(sections, journey, caravanMembers);
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

        internal static void ApplyAllTrade(List<Tradeable> tradeables)
        {
            foreach (var trans in tradeables.Where(x => x.TraderWillTrade))
                FilterHelper.SetMinAmount(trans);
        }

        internal static void ApplyGoodsTrade(List<Tradeable> tradeables)
        {
            foreach (var trans in tradeables.Where(x => x.TraderWillTrade))
            {
                if (FilterHelper.DoFiltersApply(goods, trans)) FilterHelper.SetMinAmount(trans);
                else FilterHelper.SetAmount(trans, 0);
            }
        }

        internal static void ApplyNoneTrade(List<Tradeable> tradeables)
        {
            foreach (var trans in tradeables)
                FilterHelper.SetAmount(trans, 0);
        }


    }
}
