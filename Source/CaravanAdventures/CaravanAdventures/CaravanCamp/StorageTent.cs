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
        public StorageTent()
        {
            this.CoordSize = 2;
        }

        public override void Build(Map map)
        {
            base.Build(map);
        }

        public virtual void CreateZone(Map map)
        {
            var zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {

        }
    }
}
