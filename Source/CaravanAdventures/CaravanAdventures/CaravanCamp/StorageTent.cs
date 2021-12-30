using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class StorageTent : Tent, IZoneTent
    {
        protected Zone_Stockpile zone;
        public StorageTent()
        {
            CoordSize = 2;
            SupplyCost = ModSettings.campSupplyCostStorageTent; // 3
        }
        public virtual Zone GetZone() => zone;

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var beacon = ThingMaker.MakeThing(CampDefOf.CAOrbitalTradeBeacon);
            var beaconThing = CampHelper.PrepAndGenerateThing(beacon, CellRect.CenterCell, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(beaconThing, -1);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);
        }

        public virtual void CreateZone(Map map)
        {
            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter.SetAllow(SpecialThingFilterDef.Named("AllowBiocodedWeapons"), false);
            zone.settings.filter.SetAllow(SpecialThingFilterDef.Named("AllowDeadmansApparel"), false);
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
        }

        public virtual void ApplyInventory(Map map, Caravan caravan)
        {
            if (!ModSettings.generateStorageForAllInventory) return;
            Helper.RunSavely(() => {
                var orderedItems = CaravanInventoryUtility.AllInventoryItems(caravan).OrderByDescending(x => x.def?.thingCategories?.FirstOrDefault() != null).ThenBy(x => x.def?.thingCategories?.FirstOrDefault().defName).ThenBy(x => x.MarketValue).ToList();
                foreach (var cell in zone.Cells.Reverse<IntVec3>())
                {
                    if (orderedItems.Count == 0) break;
                    var stack = orderedItems.Pop();
                    if (stack == null) break;
                    //DLog.Message($"Tent {this.Coords.First().x} {this.Coords.First().z} placing: {stack.Label}");
                    if (!cell.Filled(map)) GenDrop.TryDropSpawn_NewTmp(stack, cell, map, ThingPlaceMode.Direct, out var result);
                }
            });
        }
    }
}
