using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class AnimalArea : CampArea, IAreaRestrictionTent, IZoneTent, IShelfTent
    {
        protected Area_Allowed animalArea;
        protected Zone_Stockpile zone;
        protected bool tribal = false;
        protected Building_Storage shelf;

        public AnimalArea()
        {
            CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = 1;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            var entranceCells = CellRect.EdgeCells.Where(cell => cell.z == CellRect.minZ && cell.x == CellRect.minX + Convert.ToInt32(CellRect.Width / 2));
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("FenceGate"), ThingDefOf.WoodLog), entranceCells.First(), map, default, campAssetListRef);

            foreach (var edgeCell in CellRect.EdgeCells)
            {
                if (entranceCells.Contains(edgeCell)) continue;
                var thing = ThingMaker.MakeThing(ThingDefOf.Fence, ThingDefOf.WoodLog);
                thing.SetFaction(Faction.OfPlayer);
                campAssetListRef.Add(GenSpawn.Spawn(thing, edgeCell, map));
            }

            var innerCells = CellRect.Cells.Where(cell => cell.z == CellRect.maxZ - 1).ToArray();
            for (int i = 0; i < 6; i++)
            {
                if (i == 0 || i % 2 == 0) continue;
                var animalPlace = CampHelper.PrepAndGenerateThing(tribal
                    ? ThingMaker.MakeThing(ThingDef.Named("AnimalSleepingSpot"))
                    : ThingMaker.MakeThing(ThingDef.Named("AnimalBed"), ThingDefOf.Leather_Plain), innerCells[i], map, default, campAssetListRef) as Building_Bed;
                if (animalPlace != null) animalPlace.Medical = true;
            }

            if (!tribal)
            {
                var cellSpotShelf = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 2);
                //map.thingGrid.ThingAt(cellSpotShelf, ThingCategory.Building).Destroy();

                shelf = (Building_Storage)ThingMaker.MakeThing(ThingDef.Named("Shelf"), ThingDefOf.WoodLog);
                shelf.SetFaction(Faction.OfPlayer);
                shelf.GetStoreSettings().filter = new ThingFilter();
                if (!ModSettings.useAnimalOnlyFoodForAnimalArea) shelf.GetStoreSettings().filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);
                shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.Hay, true);
                shelf.GetStoreSettings().filter.SetAllow(ThingDefOf.Kibble, true);
                campAssetListRef.Add(GenSpawn.Spawn(shelf, cellSpotShelf, map, Rot4.West, WipeMode.Vanish));
            }

            //var targetCells = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell));
            //if (!targetCells.Any(x => x.GetTerrain(map) == TerrainDefOf.Bridge) && targetCells.All(x => x.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy)))
            //    foreach (var cell in targetCells) map.terrainGrid.SetTerrain(cell, DefDatabase<TerrainDef>.GetNamedSilentFail("StrawMatting"));

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("PenMarker"), ThingDefOf.WoodLog), CellRect.CenterCell, map, default, campAssetListRef);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            tribal = true;
            Build(map, campAssetListRef);
        }

        public virtual void CreateNewRestrictionArea(Map map, Caravan caravan)
        {
            // only works with 1 animal area right now!
            animalArea = new Area_Allowed(map.areaManager);
            map.areaManager.AllAreas.Add(animalArea);
            animalArea.SetLabel("CAAnimalAreaLabel".Translate());
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => animalArea[cell] = true);
            animalArea.AreaUpdate();
        }

        public virtual void AssignPawnsToAreas(Map map, Caravan caravan)
        {
            if (ModSettings.letAnimalsRunFree) return;
            foreach (var animal in caravan.PawnsListForReading.Where(pawn => pawn.RaceProps.Animal)) animal.playerSettings.AreaRestriction = animalArea;
        }

        public virtual Zone GetZone() => zone;

        public virtual void CreateZone(Map map)
        {
            if (!tribal) return;
            var zoneCells = CellRect.Height > CellRect.Width
                ? CellRect.Cells.Where(cell => cell.z == CellRect.maxZ - 1 && !CellRect.EdgeCells.Contains(cell))
                : CellRect.Cells.Where(cell => cell.x == CellRect.maxX - 1 && !CellRect.EdgeCells.Contains(cell));

            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter = new ThingFilter();
            if (!ModSettings.useAnimalOnlyFoodForAnimalArea) zone.settings.filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);
            zone.settings.filter.SetAllow(ThingDefOf.Hay, true);
            zone.settings.filter.SetAllow(ThingDefOf.Kibble, true);
            zone.settings.Priority = StoragePriority.Important;
            zone.label = "CAAnimalFoodZoneLabel".Translate();
            zoneCells.ToList().ForEach(cell => zone.AddCell(cell));
        }

        public void FillShelfs(Map map, Caravan caravan)
        {
            if (shelf == null || ResearchProjectDef.Named("ComplexFurniture")?.ProgressPercent != 1f) return;
            foreach (var cell in shelf.AllSlotCells())
            {
                var medicine = caravan.AllThings.FirstOrDefault(thing => (new[] {ThingDefOf.Hay, ThingDefOf.Kibble}).Contains(thing.def));
                if (medicine == null) medicine = caravan.AllThings.FirstOrDefault(thing => thing.HasThingCategory(ThingCategoryDefOf.PlantFoodRaw));
                if (medicine == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn(medicine, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

        public Building_Storage GetShelf() => shelf;

        public virtual void ApplyInventory(Map map, Caravan caravan)
        {
            if (!tribal || zone == null) return;
            foreach (var cell in zone.Cells)
            {
                var stack = caravan.AllThings.Where(x => x.def == ThingDefOf.Kibble).OrderByDescending(x => x.stackCount).FirstOrDefault()
                    ?? caravan.AllThings.Where(x => x.def == ThingDefOf.Hay).OrderByDescending(x => x.stackCount).FirstOrDefault()
                    ?? (ModSettings.useAnimalOnlyFoodForAnimalArea ? null : CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, new[] { ThingCategoryDefOf.PlantFoodRaw }));

                if (stack == null) stack = map.zoneManager.AllZones.OfType<Zone_Stockpile>()
                    .FirstOrDefault(zone => zone != this.zone && zone.AllContainedThings.Any(thing => (new List<ThingDef> { ThingDefOf.Kibble, ThingDefOf.Hay }).Contains(thing?.def)))
                    ?.AllContainedThings?.Where(thing => (new List<ThingDef> { ThingDefOf.Kibble, ThingDefOf.Hay }).Contains(thing?.def)).OrderByDescending(x => x.stackCount).FirstOrDefault();

                if (stack == null) break;
                if (!cell.Filled(map))
                {
                    if (!stack.Spawned) GenDrop.TryDropSpawn(stack, cell, map, ThingPlaceMode.Direct, out var result);
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
