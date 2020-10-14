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
    class FoodTent : Tent, IZoneTent
    {
        public FoodTent()
        {
            this.CoordSize = 2;
            SupplyCost = 3;
        }

        public override void Build(Map map)
        {
            base.Build(map);
            var cacoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var cacooler = GenSpawn.Spawn(CampThingDefOf.CACooler, cacoolerPos, map);
            cacooler.SetFaction(Faction.OfPlayer);
            var refuelComp = cacooler.TryGetComp<CompRefuelable>();
            // todo check if caravan has fuel
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());
        }

        public virtual void CreateZone(Map map)
        {
            var zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            zone.settings.filter = new ThingFilter();
            zone.settings.filter.SetAllow(ThingCategoryDefOf.Foods, true);
            zone.settings.Priority = StoragePriority.Preferred;
            // todo translate
            zone.label = "Food".Translate();
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {

        }
    }
}
