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
            SupplyCost = 3;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && (cell.x == CellRect.minX + 2 || cell.x == CellRect.maxX - 3)).ToArray();

            for (int i = 0; i < cellSpots.Length; i++)
            {
                    var dbThing = ThingMaker.MakeThing(ThingDef.Named("HydroponicsBasin"));
                    CampHelper.PrepAndGenerateThing(dbThing, cellSpots[i], map, Rot4.East, campAssetListRef);
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.y == CellRect.CenterCell.y && cell.x == CellRect.CenterCell.x);
            var caheater = GenSpawn.Spawn(CampDefOf.CAAirConditioningHeater, caheaterPos, map);
            caheater.SetFaction(Faction.OfPlayer);
            campAssetListRef.Add(caheater);
            var refuelComp = caheater.TryGetComp<CompRefuelable>();
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            // todo
            return;
           
        }
    }
}
