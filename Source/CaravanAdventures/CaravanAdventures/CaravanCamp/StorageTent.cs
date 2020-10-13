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
            var zone = new Zone_Stockpile();
            zone.settings.SetFromPreset(StorageSettingsPreset.DefaultStockpile);
            Log.Message($"settings null {zone?.settings == null} filter null {zone?.settings?.filter == null}");
            CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
            map.zoneManager.RegisterZone(zone);
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {

        }
    }
}
