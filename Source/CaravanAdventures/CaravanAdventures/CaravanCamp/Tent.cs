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
            var entranceCells = CellRect.EdgeCells.Where(cell => {
                if (CoordSize == 1) return cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2);
                else if (CoordSize == 2) return cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2)
                    || cell.z == CellRect.maxZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2);
                else return cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2)
                    || cell.z == CellRect.maxZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2)
                    || cell.x == CellRect.minX && cell.z == CellRect.minZ + Convert.ToInt32(CellRect.Height / 2)
                    || cell.x == CellRect.maxX && cell.z == CellRect.minZ + Convert.ToInt32(CellRect.Height / 2);
            });

            foreach (var cell in entranceCells)
            {
                var door = ThingMaker.MakeThing(ThingDefOf.Door, RimWorld.ThingDefOf.WoodLog);
                door.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(door, cell, map);
            }

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (entranceCells.Contains(edgeCell)) continue;
                var thing = ThingMaker.MakeThing(CampDefOf.CATentWall, CampDefOf.CASpacerTentFabric);
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
