using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class ProductionTent : Tent, IRecipeHolder, IShelfTent
    {
        protected Building_WorkTable tableButcher;
        protected Building_WorkTable handTailoringBench;
        protected Building_WorkTable refinery;
        protected Building_Storage shelf;

        public ProductionTent()
        {
            CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = ModSettings.campSupplyCostProductionTent; // 4
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("SimpleResearchBench"), ThingDefOf.WoodLog), location, map, Rot4.West, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 3);
            if (ModSettings.preferStonecutting && ResearchProjectDef.Named("Stonecutting")?.ProgressPercent == 1f) CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableStonecutter"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);
            else CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableSculpting"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 3);
            tableButcher = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableButcher"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 4 && cell.z == CellRect.minZ + 1);
            var comms = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CAMiniCommsConsole), location, map, Rot4.North, campAssetListRef);
            CampHelper.RefuelByPerc(comms, -1);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 1);
            shelf = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef) as Building_Storage;
            shelf.GetStoreSettings().filter = new ThingFilter();
            shelf.GetStoreSettings().filter.SetAllow(ThingCategoryDefOf.Leathers, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingCategoryDef.Named("Textiles"), true);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
            refinery = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("CASpacerBiofuelRefinery")), location, map, Rot4.South, campAssetListRef) as Building_WorkTable;
            CampHelper.RefuelByPerc(refinery, -1);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
            handTailoringBench = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HandTailoringBench"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 3 && cell.z == CellRect.minZ + 1);
            var heater = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CAAirConditioningHeater), location, map, Rot4.South, campAssetListRef);
            CampHelper.RefuelByPerc(heater, -1);

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 3, 0, CellRect.minZ + 2), map, Rot4.West, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 4, 0, CellRect.minZ + 2), map, Rot4.North, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 5, 0, CellRect.minZ + 2), map, Rot4.South, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 2, 0, CellRect.minZ + 2), map, Rot4.East, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 3, 0, CellRect.minZ + 2), map, Rot4.North, campAssetListRef);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);

            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("SimpleResearchBench"), ThingDefOf.WoodLog), location, map, Rot4.West, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 3);
            if (ModSettings.preferStonecutting && ResearchProjectDef.Named("Stonecutting")?.ProgressPercent == 1f) CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableStonecutter"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);
            else CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableSculpting"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 3);
            tableButcher = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableButcher"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef) as Building_WorkTable;

            if (ResearchProjectDef.Named("ComplexClothing")?.ProgressPercent == 1f)
            {
                location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
                handTailoringBench = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HandTailoringBench"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef) as Building_WorkTable;
            }
            else
            {
                location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
                handTailoringBench = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("CraftingSpot")), location, map, Rot4.East, campAssetListRef) as Building_WorkTable;
            }

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
            var torch = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.TorchLamp), location, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(torch, -1);

            var passiveCoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 4 && cell.z == CellRect.minZ + 1);
            var cooler = CampHelper.PrepAndGenerateThing(ThingDefOf.PassiveCooler, passiveCoolerPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(cooler, -1);

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 4, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 2, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
        }

        public virtual void ApplyRecipes(Caravan caravan)
        {
            var bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh")) { repeatMode = BillRepeatModeDefOf.Forever };
            bill.ingredientFilter.SetAllow(ThingCategoryDefOf.CorpsesInsect, ModSettings.campStorageAndJobsAllowHumanMeat);
            bill.ingredientFilter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, ModSettings.campStorageAndJobsAllowHumanMeat);
            tableButcher.BillStack.AddBill(bill);

            var pantsBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Pants")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(pantsBill);

            var shirtBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_CollarShirt")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(shirtBill);

            var dusterBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Duster")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(dusterBill);

            var parkaBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Parka")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(parkaBill);

            if (caravan.PawnsListForReading.Any(col => col.IsSlave))
            {
                var slaveCollar = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Collar")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Awful, QualityCategory.Legendary) };
                handTailoringBench.BillStack.AddBill(slaveCollar);

                var slaveChainThingy = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_BodyStrap")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Awful, QualityCategory.Legendary) };
                handTailoringBench.BillStack.AddBill(slaveChainThingy);
            }

            if (refinery == null) return;

            var fuelFromCorpseBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("CAMake_ChemfuelFromCorpses")) { repeatMode = BillRepeatModeDefOf.Forever };
            fuelFromCorpseBill.ingredientFilter.SetAllow(SpecialThingFilterDef.Named("AllowCorpsesColonist"), false);
            if (ModSettings.campStorageAndJobsAllowHumanMeat) fuelFromCorpseBill.ingredientFilter.SetAllow(SpecialThingFilterDefOf.AllowFresh, false);
            if (DefDatabase<SpecialThingFilterDef>.GetNamed("AllowCorpsesSlave", false) != null) fuelFromCorpseBill.ingredientFilter.SetAllow(SpecialThingFilterDef.Named("AllowCorpsesSlave"), true);
            refinery.BillStack.AddBill(fuelFromCorpseBill);

            var fuelFromWoodBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("CAMake_ChemfuelFromWood")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 30 };
            refinery.BillStack.AddBill(fuelFromWoodBill);

            var fuelFromOrganicsBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("CAMake_ChemfuelFromOrganics")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 30 };
            refinery.BillStack.AddBill(fuelFromOrganicsBill);
        }

        public virtual void ApplyRecipesTribal(Caravan caravan)
        {
            if (ResearchProjectDef.Named("ComplexClothing")?.ProgressPercent == 1f)
            {
                ApplyRecipes(caravan);
                return;
            }
                
            var bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh")) { repeatMode = BillRepeatModeDefOf.Forever };
            bill.ingredientFilter.SetAllow(ThingCategoryDefOf.CorpsesInsect, ModSettings.campStorageAndJobsAllowHumanMeat);
            bill.ingredientFilter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, ModSettings.campStorageAndJobsAllowHumanMeat);
            tableButcher.BillStack.AddBill(bill);

            var bowBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Bow_Recurve")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(bowBill);

            var pantsBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_TribalA")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(pantsBill);

            var shirtBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_WarMask")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(shirtBill);

            if (caravan.PawnsListForReading.Any(col => col.IsSlave))
            {
                var slaveCollar = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Collar")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Awful, QualityCategory.Legendary) };
                handTailoringBench.BillStack.AddBill(slaveCollar);

                var slaveChainThingy = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_BodyStrap")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Awful, QualityCategory.Legendary) };
                handTailoringBench.BillStack.AddBill(slaveChainThingy);
            }
        }

        public virtual void FillShelfs(Map map, Caravan caravan)
        {
            foreach (var cell in shelf.AllSlotCells())
            {
                var stuff = CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, new[] { ThingCategoryDefOf.Leathers, ThingCategoryDef.Named("Textiles") }, new[] { ThingDef.Named("Leather_Thrumbo") });
                if (stuff == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn(stuff, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

        public virtual Building_Storage GetShelf() => shelf;
    }
}
