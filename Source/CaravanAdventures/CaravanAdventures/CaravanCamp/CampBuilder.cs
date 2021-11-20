using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CampBuilder
    {
        protected Map map;
        protected Caravan caravan;

        protected IntVec3 tentSize = ModSettings.tentSize;
        protected int spacer = 2;
        protected List<CampArea> campParts;
        protected IntVec3 campCenterSpot;

        protected CellRect coordSystem;
        protected CellRect campSiteRect;

        protected List<Thing> campAssetListRef;
        protected float campCost = 0;
        protected bool tribal = false;
        protected int waste;

        public CampBuilder(Caravan caravan, Map map)
        {
            this.caravan = caravan;
            this.map = map;

            campParts = new List<CampArea>();
        }

        public virtual bool GenerateCamp(bool tribal = false)
        {
            var stateBackup = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;

            CalculateTentSizes();
            CalculateTentNumbersAndAssignPawnsToTents();
            CalculateCostAndDetermineType(tribal);
            if (!AssignCampLayout())
            {
                Current.ProgramState = stateBackup;
                return false;
            }
            PaySupplyCost();
            TransformTerrain();
            GenerateBuildings();
            UpdateAreas();
            ApplyZonesAndInventory();
            GenerateRecipes();
            GiveHappyThoughts();
            MoveAnimalsToAnimalArea();
            MovePrisonersToCells();
            InformPlayerHighTechCosts();

            Current.ProgramState = stateBackup;
            return true;
        }

        private void WarnPlayerAndCancelCampBuild(ProgramState stateBackup, string warning)
        {
            throw new NotImplementedException();
        }

        public static int PreemptivelyCalculateCampCosts(Caravan caravan)
        {
            if (ModSettings.hasSupplyCostsDisabled) return 0;
            var buildingCosts = 0;
            buildingCosts += (int)Math.Ceiling(caravan.PawnsListForReading.Where(x => x.IsColonist || x.IsPrisoner).Count() / 3.0 * 3);
            if (ModSettings.hasProductionTent) buildingCosts += ModSettings.campSupplyCostProductionTent;
            if (ModSettings.hasStorageTent) buildingCosts += ModSettings.campSupplyCostStorageTent;
            if (ModSettings.hasMedicalTent) buildingCosts += ModSettings.campSupplyCostMedicalTent;
            if (ModSettings.hasAnimalArea) buildingCosts += ModSettings.campSupplyCostAnimalArea;
            if (ModSettings.hasPrisonTent) buildingCosts += ModSettings.campSupplyCostRestTent;
            if (ModSettings.hasPlantTent) buildingCosts += ModSettings.campSupplyCostPlantTent;
            buildingCosts += ModSettings.campSupplyCostFoodTent;
            buildingCosts += ModSettings.campSupplyCostCampCenter;

            if (buildingCosts <= ModSettings.maxCampSupplyCost && ModSettings.generateStorageForAllInventory)
            {
                var foodTent = new FoodTent();
                var tent = new StorageTent();
                var cellsPerTent = (tent.CoordSize * ModSettings.tentSize.x) * (ModSettings.tentSize.z - 2);
                var itemsToSubstract = foodTent == null
                    ? 0
                    : Math.Min(30, CaravanInventoryUtility.AllInventoryItems(caravan)
                        .Where(x => CampHelper.GetOrderedThingsOfCategoryFromCaravan(caravan, foodTent.GetValidFoods, foodTent.GetUnvalidFoods)
                            .Contains(x))
                        .Count());
                var tentsAmount = Math.Ceiling((CaravanInventoryUtility.AllInventoryItems(caravan).Count - itemsToSubstract) / (float)cellsPerTent);
                for (int i = 0; i < tentsAmount; i++) buildingCosts += tent.SupplyCost;
            }
            if (buildingCosts > ModSettings.maxCampSupplyCost) buildingCosts = ModSettings.maxCampSupplyCost;
            return buildingCosts;
        }

        protected virtual void CalculateTentSizes()
        {
            // based on colonists, settings and inventory
            // TODO... eventually...
        }

        protected virtual void CalculateTentNumbersAndAssignPawnsToTents()
        {
            var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist && !col.IsSlave).ToList();
            var sickColonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist && col.health.hediffSet.HasNaturallyHealingInjury()).ToList();
            var prisoners = caravan.PawnsListForReading.Where(col => col.IsPrisoner).ToList();
            var slaves = caravan.PawnsListForReading.Where(col => col.IsSlave).ToList();

            campParts.Add(new CampCenter());
            campParts.Add(new FoodTent());
            // todo change to configurable number including auto (-1)
            if (ModSettings.hasProductionTent) campParts.Add(new ProductionTent());
            if (ModSettings.hasStorageTent) campParts.Add(new StorageTent());
            if (ModSettings.hasMedicalTent) campParts.Add(new MedicalTent());
            if (ModSettings.hasAnimalArea) campParts.Add(new AnimalArea());
            if (ModSettings.hasPrisonTent) campParts.Add(new PrisonerTent());
            if (ModSettings.hasPlantTent && !tribal) campParts.Add(new PlantTent());
            if (ModSettings.generateStorageForAllInventory)
            {
                var tent = new StorageTent();
                var cellsPerTent = (tent.CoordSize * tentSize.x) * (tentSize.z - 2);
                var foodTent = campParts.OfType<FoodTent>()?.FirstOrDefault();
                var itemsToSubstract = foodTent == null
                    ? 0
                    : Math.Min(30, CaravanInventoryUtility.AllInventoryItems(caravan)
                        .Where(x => CampHelper.GetOrderedThingsOfCategoryFromCaravan(caravan, foodTent.GetValidFoods, foodTent.GetUnvalidFoods)
                            .Contains(x))
                        .Count());
                var tentsAmount = Math.Ceiling((CaravanInventoryUtility.AllInventoryItems(caravan).Count - itemsToSubstract) / (float)cellsPerTent);
                for (int i = 0; i < tentsAmount; i++) campParts.Add(new StorageTent());
            }

            List<List<Pawn>> colonistRelationShipPairs = GetRelationShipPairs(colonists);
            colonistRelationShipPairs.ForEach(couple =>
            {
                campParts.Add(new RestTent() { Occupants = new List<Pawn>() { couple[0], couple[1] } });
            });

            if (ModSettings.hasMedicalTent)
            {
                sickColonists.ForEach(sick =>
                {
                    var tentWithSpace = campParts?.OfType<MedicalTent>()?.FirstOrDefault(tent => tent.Occupants.Count < (tentSize.x * tent.CoordSize - 2));
                    if (tentWithSpace == null)
                    {
                        tentWithSpace = new MedicalTent();
                        campParts.Add(tentWithSpace);
                    }
                    tentWithSpace.Occupants.Add(sick);
                });
            }

            prisoners.ForEach(pris =>
            {
                var tentWithSpace = campParts?.OfType<PrisonerTent>()?.FirstOrDefault(tent => tent.Occupants.Count < (tentSize.x * tent.CoordSize - 2));
                if (tentWithSpace == null)
                {
                    tentWithSpace = new PrisonerTent();
                    campParts.Add(tentWithSpace);
                }
                tentWithSpace.Occupants.Add(pris);
            });

            slaves.ForEach(slave =>
            {
                var tentWithSpace = campParts?.OfType<SlaveTent>()?.FirstOrDefault(tent => tent.Occupants.Count < (tentSize.x * tent.CoordSize - 2));
                if (tentWithSpace == null)
                {
                    tentWithSpace = new SlaveTent();
                    campParts.Add(tentWithSpace);
                }
                tentWithSpace.Occupants.Add(slave);
            });

            colonists.Where(col => !colonistRelationShipPairs
                .SelectMany(pair => pair)
                .Contains(col))
                .ToList()
                .ForEach(col =>
                {
                    var tentWithSpace = campParts?.OfType<RestTent>()?.FirstOrDefault(tent => tent.Occupants.Count < (tentSize.x * tent.CoordSize - 2) && !(tent is MedicalTent) && !(tent is PrisonerTent) && !(tent is SlaveTent));
                    if (tentWithSpace == null)
                    {
                        tentWithSpace = new RestTent();
                        campParts.Add(tentWithSpace);
                    }
                    tentWithSpace.Occupants.Add(col);
                });
        }

        protected virtual void CalculateCostAndDetermineType(bool tribal = false)
        {
            if (tribal)
            {
                this.tribal = true;
                return;
            }
            campParts.ForEach(part => campCost += part.SupplyCost);
            if (ModSettings.hasSupplyCostsDisabled) campCost = 0;
            else if (campCost > ModSettings.maxCampSupplyCost) campCost = ModSettings.maxCampSupplyCost;
            var amount = caravan.AllThings?.Where(thing => thing.def == CampDefOf.CASpacerTentSupplies)?.Select(thing => thing?.stackCount)?.Sum();
            if (!ModSettings.hasSupplyCostsDisabled && (amount == null || amount == 0 || campCost > amount))
            {
                this.tribal = true;
                campParts.RemoveAll(tent => tent.GetType() == typeof(PlantTent));
                return;
            }
        }

        protected virtual bool AssignCampLayout()
        {
            campCenterSpot = CampHelper.FindCenterCell(map, (IntVec3 x) => x.GetRoom(map).CellCount >= 600);
            var campCenter = campParts.OfType<CampCenter>().FirstOrDefault();
            campCenter.Coords.Add(new IntVec3(0, 0, 0));
            var failedTentCounter = 0;
            coordSystem = new CellRect(0, 0, 1, 1);

            foreach (var part in campParts)
            {
                if (part is CampCenter) continue;
                part.Coords = FindPlacement(part);
            }

            foreach (var part in campParts)
            {
                part.CellRect = CalculateRect(part);
                //DLog.Message($"Calculated rect info: x: {part.CellRect.minX} - {part.CellRect.maxX} z: {part.CellRect.minZ} - {part.CellRect.maxZ}");
            }

            foreach (var part in campParts.Reverse<CampArea>())
            {
                if (part.CellRect.ExpandedBy(1 + spacer + 1).InBounds(map)) continue;
                DLog.Message($"Removing {part.GetType()?.Name} due to being out of map bounds, failedTentCounter: {failedTentCounter + 1}");
                campCost -= part.SupplyCost;
                campParts.Remove(part);
                failedTentCounter++;
            }

            if (campParts.Count == 0 || campParts.OfType<CampCenter>()?.Count() == 0)
            {
                Messages.Message("CABuildFailStoppedCampGen".Translate(), MessageTypeDefOf.NegativeEvent);
                Log.Warning("Could not create camp center due to being out of map bounds, canceling camp creation");
                return false;
            }
            if (failedTentCounter != 0) Messages.Message("CABuildFailTents".Translate(failedTentCounter), MessageTypeDefOf.NegativeEvent);

            campSiteRect = CalcCampSiteRect();

            //DLog.Message($"Mapsize: {map.Size.x} {map.Size.y} {map.Size.z} failed tent counter: {failedTentCounter}");
            //if (Helper.Debug()) DLog.Message($"Calculated full rect info: x: {campSiteRect.minX} - {campSiteRect.maxX} z: {campSiteRect.minZ} - {campSiteRect.maxZ}");
            return true;
        }

        protected virtual void PaySupplyCost()
        {
            if (tribal) return;
            waste = ModSettings.hasSupplyCostsDisabled ? 0 : campParts.Where(part => part is RestTent || part is ProductionTent).ToList().Count;
            var remaining = Convert.ToInt32(campCost);
            var materials = CaravanInventoryUtility.TakeThings(caravan, (Func<Thing, int>)delegate (Thing thing)
            {
                if (thing.def != CampDefOf.CASpacerTentSupplies) return 0;
                var taken = Mathf.Min(remaining, thing.stackCount);
                remaining -= taken;
                return taken;
            });
            foreach (var mat in materials.Reverse<Thing>()) mat.Destroy();
        }

        protected virtual void TransformTerrain()
        {
            foreach (var c in campSiteRect.ExpandedBy(1).Cells)
            {
                var room = c.GetRoom(map);

                if (room != null && room.CellCount < 700 && room.ContainsThing(ThingDefOf.AncientCryptosleepCasket))
                {
                    foreach (var roomCell in room.Cells)
                    {
                        foreach (var thing in map.thingGrid.ThingsListAt(roomCell).Reverse<Thing>()) if (thing.def.destroyable) thing.Destroy();
                    }
                    CaravanStory.StoryUtility.FloodUnfogAdjacent(room.Map.fogGrid, room.Map, room.Cells.FirstOrDefault(cell => !room.BorderCells.Contains(cell)));

                    var roomRect = CellRect.FromLimits(room.Cells.MinBy(cell => cell.x + cell.z), room.Cells.MaxBy(cell => cell.x + cell.z));
                    foreach (var cell in roomRect.ExpandedBy(1).Cells) if (cell.Fogged(map)) map.fogGrid.Unfog(cell);
                }
            }

            foreach (var c in campSiteRect.Cells)
            {
                foreach (var thing in map.thingGrid.ThingsListAt(c).Reverse<Thing>()) if (thing.def.destroyable && thing.def?.category != ThingCategory.Plant && thing.def?.altitudeLayer != AltitudeLayer.LowPlant) thing.Destroy();
                map.roofGrid.SetRoof(c, null);
                map.fogGrid.Unfog(c);
                var terrain = map.terrainGrid.TerrainAt(c);
                if (!terrain.affordances.Any(x => (new[] { TerrainAffordanceDefOf.Bridgeable, TerrainAffordanceDefOf.Diggable, TerrainAffordanceDefOf.Light }).Contains(x))) map.terrainGrid.SetTerrain(c, TerrainDefOf.Gravel);
                else if (terrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) && !terrain.affordances.Contains(TerrainAffordanceDefOf.Light)) map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
            }
            campSiteRect.ExpandedBy(1).EdgeCells.ToList().ForEach(cell => map.fogGrid.Unfog(cell));
        }

        protected virtual CellRect CalcCampSiteRect()
        {
            var width = campParts.Max(p => p.CellRect.maxX) - campParts.Min(p => p.CellRect.minX);
            var height = campParts.Max(p => p.CellRect.maxZ) - campParts.Min(p => p.CellRect.minZ);
            return new CellRect(campParts.Min(p => p.CellRect.minX) - 1 - spacer,
                campParts.Min(p => p.CellRect.minZ) - 1 - spacer,
                width + (1 + spacer) * 2 + 1,
                height + (1 + spacer) * 2 + 1);
        }

        protected virtual CellRect CalculateRect(CampArea part)
        {
            // todo merge
            CellRect rect = default;
            if (part.CoordSize == 1)
            {
                var newCenterX = campCenterSpot.x + part.Coords.FirstOrDefault().x * (tentSize.x + spacer);
                var newCenterZ = campCenterSpot.z + part.Coords.FirstOrDefault().z * (tentSize.z + spacer);
                rect = CellRect.CenteredOn(new IntVec3(newCenterX, 0, newCenterZ), tentSize.x, tentSize.z);
            }
            else
            {
                var rects = new List<CellRect>();
                foreach (var coordinate in part.Coords)
                {
                    var newCenterX = campCenterSpot.x + coordinate.x * (tentSize.x + spacer);
                    var newCenterZ = campCenterSpot.z + coordinate.z * (tentSize.z + spacer);
                    rects.Add(CellRect.CenteredOn(new IntVec3(newCenterX, 0, newCenterZ), tentSize.x, tentSize.z));
                }
                rect = new CellRect(rects.Min(cr => cr.minX),
                    rects.Min(cr => cr.minZ),
                    rects.Max(cr => cr.maxX) - rects.Min(cr => cr.minX) + 1,
                    rects.Max(cr => cr.maxZ) - rects.Min(cr => cr.minZ) + 1);
            }
            return rect;
        }

        protected virtual List<IntVec3> FindPlacement(CampArea part)
        {
            var placementCells = new List<IntVec3>();
            var center = new IntVec3(0, 0, 0);
            if (FindFreeCoords().Count() == 0) coordSystem = coordSystem.ExpandedBy(1);
            var free = FindFreeCoords().OrderBy(coord => coord.DistanceTo(center));
            if (part.CoordSize > 1)
            {
                for (; ; )
                {
                    foreach (var cell in free)
                    {
                        var cells = GetNeigbourCells(cell, free, part.CoordSize, part.ForcedTentDirection);
                        if (cells != null)
                        {
                            placementCells = cells;
                            break;
                        }
                    }
                    if (placementCells.Count != 0) break;
                    coordSystem = coordSystem.ExpandedBy(1);
                    free = FindFreeCoords().OrderBy(coord => coord.DistanceTo(center));
                }
            }
            else
            {
                var selected = free.FirstOrDefault();
                placementCells.Add(selected);
            }

            placementCells.ForEach(selected => DLog.Message($"Selected: {selected.x} {selected.z} for {part.GetType()}"));
            return placementCells;
        }

        protected virtual List<IntVec3> GetNeigbourCells(IntVec3 cell, IOrderedEnumerable<IntVec3> source, int limit = 0, ForcedTentDirection tentDirection = ForcedTentDirection.None)
        {
            var result = new List<IntVec3>() { cell };
            for (; ; )
            {
                var neighbour = source.FirstOrDefault(cur =>
                    result.Any(res => cur.AdjacentToCardinal(res) && !result.Contains(cur))
                    && result.All(res => tentDirection == ForcedTentDirection.None
                        ? (cur.x == res.x || cur.z == res.z)
                        : tentDirection == ForcedTentDirection.Horizontal
                            ? cur.z == res.z
                            : cur.x == res.x));
                if (neighbour == default || limit != 0 && result.Count == limit) break;
                result.Add(neighbour);
            }

            if (limit != 0 && result.Count < limit) return null;
            return result;
        }

        protected virtual IEnumerable<IntVec3> FindFreeCoords()
        {
            for (int z = coordSystem.Max(cell => cell.z); z >= coordSystem.Min(cell => cell.z); z--)
            {
                for (int x = coordSystem.Min(cell => cell.x); x <= coordSystem.Max(cell => cell.x); x++)
                {
                    if (!campParts.SelectMany(p => p.Coords).Any(c => c.x == x && c.z == z)) yield return new IntVec3(x, 0, z);
                }
            }
        }

        protected virtual List<List<Pawn>> GetRelationShipPairs(List<Pawn> colonists)
        {
            var pairList = new List<List<Pawn>>();
            var prodColList = colonists.ToList();
            for (; ; )
            {
                var selCol = prodColList.FirstOrDefault(col => prodColList.Any(otherCol => otherCol != col && (new[] {
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse),
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover),
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance)}).Contains(otherCol)));

                if (selCol == null) break;
                var otherPawn = CampHelper.ExistingColonistLovePartner(selCol, colonists);
                pairList.Add(new List<Pawn> { selCol, otherPawn });
                prodColList.Remove(selCol);
                prodColList.Remove(otherPawn);
            }

            return pairList;
        }

        protected virtual void GenerateBuildings()
        {
            campAssetListRef = new List<Thing>();
            foreach (var part in campParts)
            {
                if (tribal) part.BuildTribal(map, campAssetListRef);
                else part.Build(map, campAssetListRef);
            }

            for (int i = 0; i < campSiteRect.EdgeCells.Count() - 4; i++)
            {
                if (i % 10 != 0) continue;
                var lamp = GenSpawn.Spawn(RimWorld.ThingDefOf.TorchLamp, campSiteRect.EdgeCells.ToArray()[i], map);
                lamp.SetFaction(Faction.OfPlayer);
                var fuelComp = lamp.TryGetComp<CompRefuelable>();
                if (fuelComp != null) fuelComp.allowAutoRefuel = false;
            }

            var center = campParts.FirstOrDefault(part => part is CampCenter) as CampCenter;
            var comp = center.Control.TryGetComp<CompCampControl>();
            comp.CampRects = campParts.Select(part => part.CellRect).ToList();
            comp.CampAssets = campAssetListRef;
            comp.ResourceCount = Convert.ToInt32(campCost);
            comp.Tribal = tribal;
            comp.Waste = waste;
        }

        protected virtual void UpdateAreas()
        {
            foreach (var cell in campSiteRect)
            {
                map.areaManager.Home[cell] = true;
                if (ModSettings.autoApplyCampClearSnowArea) if (!cell.Roofed(map)) map.areaManager.SnowClear[cell] = true;
            }

            foreach (var tent in campParts.OfType<Tent>())
            {
                if (tent is RestTent) CampHelper.BringToNormalizedTemp(tent.CellRect, map);

                foreach (var cell in tent.CellRect.Cells)
                {
                    map.areaManager.BuildRoof[cell] = true;
                    map.areaManager.NoRoof[cell] = false;
                }
            }

            foreach (var animalArea in campParts.OfType<AnimalArea>())
            {
                foreach (var cell in animalArea.CellRect.Cells)
                {
                    map.areaManager.BuildRoof[cell] = false;
                    map.areaManager.NoRoof[cell] = true;
                }
            }
        }

        protected virtual void ApplyZonesAndInventory()
        {
            foreach (var zoneTent in campParts.OfType<IZoneTent>())
            {
                zoneTent.CreateZone(map);
                if (!(zoneTent is StorageTent)) zoneTent.ApplyInventory(map, caravan);
            }

            foreach (var shelfTent in campParts.OfType<IShelfTent>())
            {
                if (tribal) break;
                shelfTent.FillShelfs(map, caravan);
            }

            foreach (var storageTent in campParts.OfType<StorageTent>()) storageTent.ApplyInventory(map, caravan);

            foreach (var areaRestrictionTent in campParts.OfType<IAreaRestrictionTent>())
            {
                areaRestrictionTent.CreateNewRestrictionArea(map, caravan);
                areaRestrictionTent.AssignPawnsToAreas(map, caravan);
            }

            CampHelper.AddAnimalFreeAreaRestriction(campParts.Where(part => part is FoodTent || part is PlantTent), map, caravan, ModSettings.letAnimalsRunFree);
        }

        protected virtual void GenerateRecipes()
        {
            if (!ModSettings.autoApplyCampGearRecipes) return;
            foreach (var recipeHolder in campParts.OfType<IRecipeHolder>())
            {
                if (tribal) recipeHolder.ApplyRecipesTribal(caravan);
                else recipeHolder.ApplyRecipes(caravan);
            }
        }

        protected virtual void GiveHappyThoughts() => caravan.PawnsListForReading.Where(pawn => pawn.IsColonist).ToList().ForEach(pawn => pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("CACamping")));

        protected virtual void MoveAnimalsToAnimalArea()
        {
            var animalArea = campParts.OfType<AnimalArea>().FirstOrDefault();
            if (animalArea == null) return;
            var animals = caravan.PawnsListForReading.Where(pawn => pawn?.RaceProps?.Animal == true);
            foreach (var animal in animals.Reverse())
            {
                if (ModSettings.letAnimalsRunFree && animal?.RaceProps?.trainability != null && animal?.RaceProps?.trainability != TrainabilityDefOf.None) continue;
                Helper.RunSavely(() => PlaceIntoAnimalArea(animalArea, animal));
            }
        }

        protected virtual void PlaceIntoAnimalArea(AnimalArea animalArea, Pawn animal)
        {
            var cell = animalArea.CellRect.Cells.Where(c => !animalArea.CellRect.EdgeCells.Contains(c)).RandomElement();
            if (cell == default) return;
            caravan.RemovePawn(animal);
            caravan.Notify_PawnRemoved(animal);
            GenSpawn.Spawn(animal, cell, map, Rot4.Random);
            if (!animal.Spawned)
            {
                Log.Warning($"Unable to place {animal.NameShortColored} in camp area.");
                caravan.AddPawn(animal, true);
                caravan.Notify_PawnAdded(animal);
                return;
            }
            if (ModSettings.generateStorageForAllInventory) animal.inventory.DropAllNearPawn(animal.Position);
            animal.jobs.StartJob(JobMaker.MakeJob(JobDefOf.GotoWander, null), Verse.AI.JobCondition.InterruptForced, null, false, true, null, new Verse.AI.JobTag?(Verse.AI.JobTag.Idle), false, false);
        }

        protected virtual void MovePrisonersToCells()
        {
            var prisoners = caravan.PawnsListForReading.Where(col => col.IsPrisoner).ToList();
            var beds = map.listerBuildings.allBuildingsColonist.OfType<Building_Bed>().Where(x => x.ForPrisoners);
            foreach (var bed in beds)
            {
                var prisoner = prisoners.FirstOrDefault();
                if (prisoner == null) break;
                GetIntoBed(prisoner, bed);
                prisoners.Remove(prisoner);
            }
        }

        protected virtual void InformPlayerHighTechCosts()
        {
            if (!tribal) Messages.Message("CAEstablishedCampNotification".Translate(campCost), MessageTypeDefOf.PositiveEvent);
        }

        protected virtual void GetIntoBed(Pawn pawn, Building_Bed bed)
        {
            var pos = RestUtility.GetBedSleepingSlotPosFor(pawn, bed);
            caravan.RemovePawn(pawn);
            caravan.Notify_PawnRemoved(pawn);
            GenSpawn.Spawn(pawn, pos, map);
            if (ModSettings.generateStorageForAllInventory) pawn.inventory.DropAllNearPawn(pawn.Position);
            pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, bed), Verse.AI.JobCondition.InterruptForced, null, false, true, null, new Verse.AI.JobTag?(Verse.AI.JobTag.TuckedIntoBed), false, false);
        }


    }
}
