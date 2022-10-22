using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    enum ShelfDirection { Horizontal, Vertical }
    public class ShelfStorageTent : Tent, IMultiShelfTent
    {
        protected List<Building_Storage> shelfs;
        public ShelfStorageTent()
        {
            CoordSize = 2;
            SupplyCost = ModSettings.campSupplyCostStorageTent; // 3
            shelfs = new List<Building_Storage>();
        }
        public virtual List<Building_Storage> GetShelfs() => shelfs;

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var beacon = ThingMaker.MakeThing(CampDefOf.CAOrbitalTradeBeacon);
            var beaconThing = CampHelper.PrepAndGenerateThing(beacon, CellRect.CenterCell, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(beaconThing, -1);

            if (!ModSettings.useStorageShelfs) return;
            if (CellRect.Width > CellRect.Height) BuildShelfs(map, campAssetListRef, ShelfDirection.Horizontal);
            else BuildShelfs(map, campAssetListRef, ShelfDirection.Vertical);

            LinkShelfs(map);
        }

        private void LinkShelfs(Map map)
        {
            var group = map.storageGroups.NewGroup();
            if (!shelfs?.Any() ?? true) return;
            group.InitFrom(shelfs.FirstOrDefault());
            foreach (var shelf in shelfs)
            {
                if (shelf == null) continue;
                shelf.SetStorageGroup(group);
            }
        }

        private void BuildShelfs(Map map, List<Thing> campAssetListRef, ShelfDirection shelfDirection)
        {
            switch (shelfDirection)
            {
                case ShelfDirection.Vertical:
                    for (int i = 0; i < 5; i++)
                    {
                        var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 2 - (2 * i));
                        if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotShelf, Rot4.West)) continue;
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.maxZ - 1 - (2 * i));
                        if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotShelf, Rot4.East)) continue;
                    }
                    break;
                case ShelfDirection.Horizontal:
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == 2)
                        {
                            var cellSpotSingle = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 2 - (2 * i) && cell.z == CellRect.maxZ - 1);
                            if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotSingle, Rot4.South, true)) continue;
                            continue;
                        }
                        var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 - (2 * i) && cell.z == CellRect.maxZ - 1);
                        if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotShelf, Rot4.South)) continue;
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == 2)
                        {
                            var cellSpotSingle = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 2 - (2 * i) && cell.z == CellRect.maxZ - 3);
                            if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotSingle, Rot4.North, true)) continue;
                            continue;
                        }
                        var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 2 - (2 * i) && cell.z == CellRect.maxZ - 3);
                        if (!GenerateShelf(map, campAssetListRef, shelfDirection, cellSpotShelf, Rot4.North)) continue;
                    }
                    var singleShelfSpotEast = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 1 && cell.z == CellRect.minZ + 2);
                    GenerateShelf(map, campAssetListRef, shelfDirection, singleShelfSpotEast, Rot4.East, true);
                    var singleShelfSpotWest = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
                    GenerateShelf(map, campAssetListRef, shelfDirection, singleShelfSpotWest, Rot4.West, true);
                    //var hss = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 1);
                    break;
            }
        }

        private bool GenerateShelf(Map map, List<Thing> campAssetListRef, ShelfDirection shelfDirection, IntVec3 cellSpotShelf, Rot4 rot, bool small = false)
        {
            var shelf = ThingMaker.MakeThing(ThingDef.Named(small ? "ShelfSmall" : "Shelf"), ThingDefOf.WoodLog) as Building_Storage;
            if (shelf == null)
            {
                DLog.Error($"Error creating shelf for storage tent, shelf is null");
                return false;
            }
            CampHelper.PrepAndGenerateThing(shelf, cellSpotShelf, map, rot, campAssetListRef);
            shelfs.Add(shelf);
            return true;
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);

            if (!ModSettings.useStorageShelfs || ResearchProjectDef.Named("ComplexFurniture")?.ProgressPercent != 1f) return;
            if (CellRect.Width > CellRect.Height) BuildShelfs(map, campAssetListRef, ShelfDirection.Horizontal);
            else BuildShelfs(map, campAssetListRef, ShelfDirection.Vertical);

            LinkShelfs(map);
        }

        public void FillShelfs(Map map, Caravan caravan)
        {
            if (!shelfs?.Any(x => x != null) ?? true) return;
            if (!ModSettings.generateStorageForAllInventory || !ModSettings.useStorageShelfs) return;
            Helper.RunSavely(() =>
            {
                var tempShelf = ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog) as Building_Storage;
                var orderedItems = CaravanInventoryUtility
                    .AllInventoryItems(caravan)
                    .Where(x => x != null && tempShelf.Accepts(x) && !(x is MinifiedThing))
                    .OrderByDescending(x => x.def?.thingCategories?.FirstOrDefault() != null)
                    .ThenBy(x => x.def?.thingCategories?.FirstOrDefault().defName)
                    .ThenBy(x => x.MarketValue).ToList();

                foreach (var cell in shelfs.SelectMany(shelf => shelf.AllSlotCellsList()).Reverse<IntVec3>())
                {
                    //DLog.Message($"trying to place {orderedItems?.LastOrDefault()?.def?.defName} in cell {cell.x} {cell.y}");
                    for (; ; )
                    {
                        if (!orderedItems?.Any() ?? true) return;
                        if (cell.GetItemCount(map) >= cell.GetMaxItemsAllowedInCell(map)) break;
                        var stack = orderedItems.Pop();
                        if (stack == null) return;
                        //DLog.Message($"Tent {this.Coords.First().x} {this.Coords.First().z} placing: {stack.Label}");
                        if (!cell.Filled(map)) GenDrop.TryDropSpawn(stack, cell, map, ThingPlaceMode.Direct, out var result);
                    }
                }
            });
        }
    }
}
