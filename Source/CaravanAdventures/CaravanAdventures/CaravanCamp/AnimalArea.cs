using HarmonyLib;
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
    class AnimalArea : CampArea, IAreaRestrictionTent, IZoneTent
    {
        private Area_Allowed animalArea;
        public AnimalArea()
        {
            CoordSize = 2;
            SupplyCost = 1;
        }

        public override void Build(Map map)
        {
            var entranceCells = CellRect.EdgeCells.Where(cell =>  cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2));

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (entranceCells.Contains(edgeCell)) continue;
                var thing = ThingMaker.MakeThing(CampDefOf.CAFencePost);
                thing.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(thing, edgeCell, map);
            }
        }

        public void CreateNewRestrictionArea(Map map, Caravan caravan)
        {
            animalArea = new Area_Allowed(map.areaManager);
            map.areaManager.AllAreas.Add(animalArea);
            animalArea.SetLabel("CAAnimalAreaLabel".Translate());
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => animalArea[cell] = true);
            animalArea.AreaUpdate();
        }

        public void AssignPawnsToAreas(Map map, Caravan caravan)
        {
            foreach (var animal in caravan.PawnsListForReading.Where(pawn => pawn.RaceProps.Animal)) animal.playerSettings.AreaRestriction = animalArea;
        }

        public Zone GetZone()
        {
            throw new NotImplementedException();
        }

        public void CreateZone(Map map)
        {
            throw new NotImplementedException();
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {
            throw new NotImplementedException();
        }
    }
}
