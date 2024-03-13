using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanItemSelection
{
    class FilterCombs
    {
        public static FilterSet packUp;
        public static FilterSet goods;
        public static FilterSet journey;

        // FilterSets only used for defaults at this point
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
                DefDatabase<ThingCategoryDef>.GetNamedSilentFail("CorpsesInsect"),
                ThingCategoryDefOf.CorpsesMechanoid })
            {
                Name = "Corpses",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDefOf.ResourcesRaw,
                ThingCategoryDefOf.StoneBlocks,
                // needs specific building category for future reference, just general building cat won't do
                //ThingCategoryDef.Named("BuildingsArt"),
                ThingCategoryDef.Named("BuildingsProduction"),
                ThingCategoryDef.Named("BuildingsFurniture"),
                ThingCategoryDef.Named("BuildingsPower"),
                ThingCategoryDef.Named("BuildingsSecurity"),
                ThingCategoryDef.Named("BuildingsMisc"),
                ThingCategoryDef.Named("BuildingsJoy"),
                ThingCategoryDef.Named("BuildingsTemperature"),
                //ThingCategoryDef.Named("BuildingsSpecial"),

            })
            {
                Name = "BuildMaterials",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingDefOf.Bedroll,
                ThingDef.Named("BedrollDouble"),
                ThingDef.Named("MegascreenTelevision"),
                ModsConfig.RoyaltyActive ? ThingDef.Named("AnimusStone") : null,
            })
            {
                Name = "BuildingsIncluded",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Include,
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
            packUp.appliedFilters.Add(new Filter(new object[] {
                ThingCategoryDef.Named("Plants")})
            {
                Name = "Trees",
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
                    //if (FilterHelper.DoFiltersApply(packUp, trans)) FilterHelper.SetMaxAmount(trans);
                    if (FilterHelper.DoesFilterApply(InitGC.packUpFilter, trans)) FilterHelper.SetMaxAmount(trans);
                    else if (InitGC.packUpExclusive) FilterHelper.SetMinAmount(trans);
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
                    //if (FilterHelper.DoFiltersApply(goods, trans)) FilterHelper.SetMaxAmount(trans);
                    //else FilterHelper.SetMinAmount(trans);

                    if (FilterHelper.DoesFilterApply(InitGC.goodsFilter, trans)) FilterHelper.SetMaxAmount(trans);
                    else if (InitGC.goodsExclusive) FilterHelper.SetMinAmount(trans);
                }
            }
        }

        public static void ApplyGoods2(List<Patches.Section> sections)
        {
            foreach (var section in sections)
            {
                foreach (var trans in section.cachedTransferables)
                {
                    if (FilterHelper.DoesFilterApply(InitGC.journeyFilter, trans)) FilterHelper.SetMaxAmount(trans);
                    else if (InitGC.journeyExclusive) FilterHelper.SetMinAmount(trans);
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
                //if (FilterHelper.DoFiltersApply(goods, trans)) FilterHelper.SetMinAmount(trans);
                //else FilterHelper.SetAmount(trans, 0);

                if (FilterHelper.DoesFilterApply(InitGC.goodsFilter, trans)) FilterHelper.SetMinAmount(trans);
                else if (InitGC.goodsExclusive) FilterHelper.SetAmount(trans, 0);
            }
        }

        public static void ApplyGoodsTrade2(List<Tradeable> tradeables)
        {
            foreach (var trans in tradeables.Where(x => x.TraderWillTrade))
            {
                //if (FilterHelper.DoFiltersApply(goods, trans)) FilterHelper.SetMinAmount(trans);
                //else FilterHelper.SetAmount(trans, 0);

                if (FilterHelper.DoesFilterApply(InitGC.journeyFilter, trans)) FilterHelper.SetMinAmount(trans);
                else if (InitGC.journeyExclusive) FilterHelper.SetAmount(trans, 0);
            }
        }

        internal static void ApplyNoneTrade(List<Tradeable> tradeables)
        {
            foreach (var trans in tradeables)
                FilterHelper.SetAmount(trans, 0);
        }


    }
}
