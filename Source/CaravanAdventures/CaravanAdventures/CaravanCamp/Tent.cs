using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    abstract class Tent : CampArea
    {
        public override void Build(Map map)
        {
            var entranceCell = CellRect.EdgeCells.OrderBy(cell => cell.z).FirstOrDefault(cell => cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2));
            var thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.Door, RimWorld.ThingDefOf.WoodLog);
            GenSpawn.Spawn(thing, entranceCell, map, Rot4.South);

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (edgeCell == entranceCell) continue;
                thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.Wall, RimWorld.ThingDefOf.WoodLog);
                GenSpawn.Spawn(thing, edgeCell, map);
            }

            foreach (var cell in CellRect.Cells)
            {
                map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
            }
        }

    }
}
