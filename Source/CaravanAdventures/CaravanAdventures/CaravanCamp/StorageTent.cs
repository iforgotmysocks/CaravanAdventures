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
    class StorageTent : Tent, IZoneTent
    {
        private Zone_Stockpile zone;
        public StorageTent()
        {
            CoordSize = 2;
        }
        public Zone GetZone() => zone;

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

        public void ApplyInventory(Map map, Caravan caravan)
        {
            if (!ModSettings.generateStorageForAllInventory) return;

            foreach (var cell in zone.Cells)
            {
                var stack = CaravanInventoryUtility.AllInventoryItems(caravan).FirstOrDefault();
                if (stack == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn_NewTmp(stack, cell, map, ThingPlaceMode.Direct, out var result);
            }

        }
    }
}
