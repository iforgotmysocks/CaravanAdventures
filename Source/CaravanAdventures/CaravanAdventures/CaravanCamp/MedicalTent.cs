using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class MedicalTent : RestTent, IShelfTent, IZoneTent
    {
        protected ThingDef[] validMedicine = new[] { ThingDefOf.MedicineUltratech, ThingDefOf.MedicineIndustrial, ThingDefOf.MedicineHerbal };
        protected Building_Storage shelf;
        protected Zone_Stockpile zone;
        public MedicalTent()
        {
            this.CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = ModSettings.campSupplyCostMedicalTent; // 4
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            SkipPawnAssignment = true;
            base.Build(map, campAssetListRef);
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.Medical = true;
            }
            var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 2);
            var cellRemoveBed = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 2 && cell.z == CellRect.maxZ - 2);
            if (CoordSize > 1) map.thingGrid.ThingAt(cellRemoveBed, ThingCategory.Building).Destroy();
            map.thingGrid.ThingAt(cellSpotShelf, ThingCategory.Building).Destroy();

            shelf = (Building_Storage)ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog);
            shelf.SetFaction(Faction.OfPlayer);
            shelf.GetStoreSettings().filter = new ThingFilter();
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineHerbal, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineIndustrial, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineUltratech, true);
            campAssetListRef.Add(GenSpawn.Spawn(shelf, cellSpotShelf, map, Rot4.West, WipeMode.Vanish));
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            SkipPawnAssignment = true;
            base.BuildTribal(map, campAssetListRef);
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.Medical = true;
            }
            var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 2);
            var cellRemoveBed = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 2 && cell.z == CellRect.maxZ - 2);
            if (CoordSize > 1) map.thingGrid.ThingAt(cellRemoveBed, ThingCategory.Building).Destroy();
            map.thingGrid.ThingAt(cellSpotShelf, ThingCategory.Building).Destroy();

            if (CoordSize > 1)
            {
                var cellSpotTorch = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
                map.thingGrid.ThingAt(cellSpotTorch, ThingCategory.Building).Destroy();

                var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
                var torch = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.TorchLamp), location, map, default, campAssetListRef);
                CampHelper.RefuelByPerc(torch, -1);
            }
        }

        public virtual void FillShelfs(Map map, Caravan caravan)
        {
            if (shelf == null) return;
            foreach (var cell in shelf.AllSlotCells())
            {
                var medicine = caravan.AllThings.FirstOrDefault(thing => validMedicine.Contains(thing.def));
                if (medicine == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn(medicine, cell, map, ThingPlaceMode.Direct, out var result); 
            }
        }

        public virtual Building_Storage GetShelf() => shelf;

        public virtual void CreateZone(Map map)
        {
            if (shelf != null) return;
            var zoneCells = CellRect.Cells.Where(cell => cell.x == CellRect.maxX - 1 && cell.z != CellRect.minZ + 1 && !CellRect.EdgeCells.Contains(cell));

            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter = new ThingFilter();
            zone.settings.filter.SetAllow(ThingDefOf.MedicineHerbal, true);
            zone.settings.filter.SetAllow(ThingDefOf.MedicineIndustrial, true);
            zone.settings.filter.SetAllow(ThingDefOf.MedicineUltratech, true);
            zone.settings.Priority = StoragePriority.Preferred;
            zone.label = "CAMedicineZone".Translate();
            zoneCells.ToList().ForEach(cell => zone.AddCell(cell));
        }

        public virtual void ApplyInventory(Map map, Caravan caravan)
        {
            if (zone == null) return;
            foreach (var cell in zone.Cells)
            {
                var medicine = caravan.AllThings.FirstOrDefault(thing => validMedicine.Contains(thing.def));
                if (medicine == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn(medicine, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

        public virtual Zone GetZone() => zone;
    }
}
