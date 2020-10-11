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
            var thing = ThingMaker.MakeThing(ThingDefOf.Door, RimWorld.ThingDefOf.WoodLog);
            thing.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(thing, entranceCell, map, Rot4.South);

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (edgeCell == entranceCell) continue;
                thing = ThingMaker.MakeThing(CampDefOf.CATentWall, CampDefOf.CASpacerTentFabric);
                thing.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(thing, edgeCell, map);
            }

            foreach (var cell in CellRect.Cells)
            {
                map.terrainGrid.SetTerrain(cell, CampDefOf.CATentFloor);
                map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
            }
        }

    }
}
