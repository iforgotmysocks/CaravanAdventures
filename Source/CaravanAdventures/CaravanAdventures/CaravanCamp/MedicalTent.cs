using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class MedicalTent : RestTent
    {
        public MedicalTent()
        {
            this.CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
        }

        public override void Build(Map map)
        {
            base.Build(map);
            // todo maybe just add a bed list to parent
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

            var shelf = (Building_Storage)ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog);
            shelf.SetFaction(Faction.OfPlayer);
            shelf.GetStoreSettings().filter = new ThingFilter();
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineHerbal, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineIndustrial, true);
            shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.MedicineUltratech, true);
            GenSpawn.Spawn(shelf, cellSpotShelf, map, Rot4.West, WipeMode.Vanish);
        }
    }
}
