using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class ProductionTent : Tent, IRecipeHolder, IShelfTent
    {
        private Building_WorkTable tableButcher;
        private Building_WorkTable handTailoringBench;
        private Building_Storage shelf;

        public ProductionTent()
        {
            CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = 4;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("SimpleResearchBench"), ThingDefOf.WoodLog), location, map, Rot4.West, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 3);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableSculpting"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 3);
            tableButcher = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableButcher"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 1);
            shelf = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog), location, map, Rot4.South, campAssetListRef) as Building_Storage;
            shelf.GetStoreSettings().filter = new ThingFilter();
            shelf.GetStoreSettings().filter.SetAllow(ThingCategoryDefOf.Leathers, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingCategoryDef.Named("Textiles"), true);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 1);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("ToolCabinet")), location, map, Rot4.South, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
            handTailoringBench = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HandTailoringBench"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
            var heater = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CAAirConditioningHeater), location, map, default, campAssetListRef);
            var refuelComp = heater.TryGetComp<CompRefuelable>();
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 3, 0, CellRect.minZ + 2), map, Rot4.West, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 4, 0, CellRect.minZ + 2), map, Rot4.North, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 2, 0, CellRect.minZ + 2), map, Rot4.East, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.DiningChair, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 3, 0, CellRect.minZ + 2), map, Rot4.North, campAssetListRef);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);

            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("SimpleResearchBench"), ThingDefOf.WoodLog), location, map, Rot4.West, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 3);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableSculpting"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 3);
            tableButcher = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableButcher"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
            handTailoringBench = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("CraftingSpot")), location, map, Rot4.East, campAssetListRef) as Building_WorkTable;

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.TorchLamp), location, map, default, campAssetListRef);

            var passiveCoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var cooler = CampHelper.PrepAndGenerateThing(ThingDefOf.PassiveCooler, passiveCoolerPos, map, default, campAssetListRef);
            var refuelComp = cooler.TryGetComp<CompRefuelable>();
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel() / 2f);

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 4, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 2, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
        }

        public void ApplyRecipes(Caravan caravan)
        {
            var bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh")) { repeatMode = BillRepeatModeDefOf.Forever };
            //bill.recipe.defaultIngredientFilter.SetAllow(ThingCategoryDefOf.CorpsesInsect, false);
            tableButcher.BillStack.AddBill(bill);

            // todo check in modsettings 
            // if (doApperalRecipes) 
         
            
            var pantsBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Pants")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(pantsBill);

            var shirtBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_CollarShirt")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(shirtBill);

            var dusterBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_Duster")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(dusterBill);
        }

        public void ApplyRecipesTribal(Caravan caravan)
        {
            var bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh")) { repeatMode = BillRepeatModeDefOf.Forever };
            //bill.recipe.defaultIngredientFilter.SetAllow(ThingCategoryDefOf.CorpsesInsect, false);
            tableButcher.BillStack.AddBill(bill);

            var bowBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Bow_Recurve")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(bowBill);

            var pantsBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_TribalA")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(pantsBill);

            var shirtBill = new Bill_ProductionWithUft(DefDatabase<RecipeDef>.GetNamed("Make_Apparel_WarMask")) { repeatMode = BillRepeatModeDefOf.TargetCount, targetCount = 1, hpRange = new FloatRange(0.9f, 1f), includeTainted = false, qualityRange = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary) };
            handTailoringBench.BillStack.AddBill(shirtBill);
        }

        public void FillShelfs(Map map, Caravan caravan)
        {
            foreach (var cell in shelf.AllSlotCells())
            {
                var stuff = CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, new[] { ThingCategoryDefOf.Leathers, ThingCategoryDef.Named("Textiles") }, new[] { ThingDef.Named("Leather_Thrumbo") });
                if (stuff == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn_NewTmp(stuff, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

        public Building_Storage GetShelf() => shelf;
    }
}
