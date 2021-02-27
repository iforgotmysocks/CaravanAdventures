using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class PlantTent : Tent
    {
        public PlantTent()
        {
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            CoordSize = 2;
            SupplyCost = ModSettings.campSupplyCostPlantTent; // 6
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && (cell.x == CellRect.minX + 2 || cell.x == CellRect.maxX - 3)).ToArray();

            for (int i = 0; i < cellSpots.Length; i++)
            {
                var basinThing = ThingMaker.MakeThing(ThingDef.Named("HydroponicsBasin"));
                var basin = CampHelper.PrepAndGenerateThing(basinThing, cellSpots[i], map, Rot4.East, campAssetListRef);

                foreach (var cell in basin.OccupiedRect().Cells)
                {
                    var plant = CampHelper.PrepAndGenerateThing(ThingDef.Named($"Plant_Rice"), cell, map, default, campAssetListRef, true) as Plant;
                    plant.Growth = 0.1f;
                    plant.sown = true;
                }
            }

            var sunThingyPos = CellRect.CenterCell;
            sunThingyPos.x -= 1;
            CampHelper.PrepAndGenerateThing(ThingDef.Named("SunLamp"), sunThingyPos, map, default, campAssetListRef);

            var generatorPos = CellRect.CenterCell;
            generatorPos.x -= 1;
            generatorPos.z -= 1;
            var generator = CampHelper.PrepAndGenerateThing(CampDefOf.CAChemfuelPoweredGenerator, generatorPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(generator, -1);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            // todo
            return;

        }
    }
}
