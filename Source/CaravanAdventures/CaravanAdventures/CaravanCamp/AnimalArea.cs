﻿using HarmonyLib;
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
        private Zone_Stockpile zone;

        public AnimalArea()
        {
            CoordSize = 2;
            SupplyCost = 1;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            var entranceCells = CellRect.EdgeCells.Where(cell => cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2));

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (entranceCells.Contains(edgeCell)) continue;
                var thing = ThingMaker.MakeThing(CampDefOf.CAFencePost);
                thing.SetFaction(Faction.OfPlayer);
                campAssetListRef.Add(GenSpawn.Spawn(thing, edgeCell, map));
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

        public Zone GetZone() => zone;

        public void CreateZone(Map map)
        {
            var zoneCells = CellRect.Height > CellRect.Width
                ? CellRect.Cells.Where(cell => cell.z == CellRect.maxZ - 1 && !CellRect.EdgeCells.Contains(cell))
                : CellRect.Cells.Where(cell => cell.x == CellRect.maxX - 1 && !CellRect.EdgeCells.Contains(cell));

            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter = new ThingFilter();
            zone.settings.filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);
            zone.settings.filter.SetAllow(ThingDefOf.Hay, true);
            zone.settings.filter.SetAllow(ThingDefOf.Kibble, true);
            zone.settings.Priority = StoragePriority.Preferred;
            zone.label = "CAAnimalFoodZoneLabel".Translate();
            zoneCells.ToList().ForEach(cell => zone.AddCell(cell));
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {
            foreach (var cell in zone.Cells)
            {
                var stack = caravan.AllThings.Where(x => x.def == ThingDefOf.Kibble).OrderByDescending(x => x.stackCount).FirstOrDefault()
                    ?? caravan.AllThings.Where(x => x.def == ThingDefOf.Hay).OrderByDescending(x => x.stackCount).FirstOrDefault()
                    ?? CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, new[] { ThingCategoryDefOf.PlantFoodRaw });

                if (stack == null) stack = map.zoneManager.AllZones.OfType<Zone_Stockpile>()
                    .FirstOrDefault(zone => zone != this.zone && zone.AllContainedThings.Any(thing => (new List<ThingDef> { ThingDefOf.Kibble, ThingDefOf.Hay }).Contains(thing?.def)))
                    ?.AllContainedThings?.Where(thing => (new List<ThingDef> { ThingDefOf.Kibble, ThingDefOf.Hay }).Contains(thing?.def)).OrderByDescending(x => x.stackCount).FirstOrDefault();

                if (stack == null) break;
                if (!cell.Filled(map))
                {
                    if (!stack.Spawned) GenDrop.TryDropSpawn_NewTmp(stack, cell, map, ThingPlaceMode.Direct, out var result);
                    else
                    {
                        stack.DeSpawn();
                        GenPlace.TryPlaceThing(stack, cell, map, ThingPlaceMode.Direct);
                    }
                }
            }
        }
    }
}