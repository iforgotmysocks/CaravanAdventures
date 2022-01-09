using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class FoodTent : Tent, IZoneTent
    {
        protected Zone_Stockpile zone;
        protected ThingCategoryDef[] validFoods = new[] { ThingCategoryDefOf.FoodMeals, ThingCategoryDefOf.Foods, ThingCategoryDefOf.MeatRaw };
        protected ThingDef[] unvalidFoods = new[] { ThingDefOf.Kibble, ThingDefOf.Hay, ThingDefOf.MealSurvivalPack };

        public FoodTent()
        {
            this.CoordSize = 2;
            SupplyCost = ModSettings.campSupplyCostFoodTent; // 3;
        }

        public virtual Zone GetZone() => zone;
        public virtual ThingCategoryDef[] GetValidFoods => validFoods;
        public virtual ThingDef[] GetUnvalidFoods => unvalidFoods;
        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var cacoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var cacooler = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CACooler), cacoolerPos, map, Rot4.South, campAssetListRef);
            CampHelper.RefuelByPerc(cacooler, -1);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);
        }

        public virtual void CreateZone(Map map)
        {
            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter = new ThingFilter();
            zone.settings.filter.SetAllow(SpecialThingFilterDef.Named("AllowRotten"), false);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.Foods, true);
            zone.settings.filter.SetAllow(ThingDefOf.MealSurvivalPack, false);
            zone.settings.filter.SetAllow(ThingDefOf.Kibble, false);
            zone.settings.filter.SetAllow(ThingDefOf.Hay, false);
            zone.settings.filter.SetAllow(ThingDef.Named("Meat_Megaspider"), ModSettings.campStorageAndJobsAllowInsectMeat);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, true);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesInsect, ModSettings.campStorageAndJobsAllowInsectMeat);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, ModSettings.campStorageAndJobsAllowHumanMeat);
            zone.settings.filter.SetAllow(ThingDefOf.Meat_Human, ModSettings.campStorageAndJobsAllowHumanMeat);
            zone.settings.Priority = StoragePriority.Preferred;
            zone.label = "CAFoodZoneLabel".Translate();
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
            //zone.CheckContiguous();
        }

        public virtual void ApplyInventory(Map map, Caravan caravan)
        {
            if (ModSettings.campStorageAndJobsAllowHumanMeat) validFoods.Append(ThingCategoryDefOf.CorpsesHumanlike);
            if (ModSettings.campStorageAndJobsAllowInsectMeat) validFoods.Append(ThingCategoryDefOf.CorpsesInsect);

            foreach (var cell in zone.Cells)
            {
                var stack = CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, validFoods, unvalidFoods);
                if (stack == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn_NewTmp(stack, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

    }
}
