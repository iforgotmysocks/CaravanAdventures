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
    // todos
    // build costs: 
    // -- fixed amount per colonists
    // -- high quality camping supplies for hq camp
    // -- normal lether and stuff low lq camp
    // -- make deconstructing items not give anything
    // -- resources are gained back by dismanteling the camp via the option of the camp fire

    // Find fabric and leather used to build buildings
    // find rect in middle of map large enough to fit tents for x pawns
    // build bridges on non compatible ground
    // generate buildings 
    // create small versions of generators etc 

    // zones
    // -- home zone
    // -- food zone
    // -- medicine shelfs -> rework medicine tent design
    // -- storage zones
    // -- animal zone?

    // functionality: 
    // - add right click option to campfire that turns all camping stuff back into resources
    // - maybe add ability to build new tents from camp supplies

    // add another field rect

    class CampBuilder
    {
        private Map map;
        private Caravan caravan;

        private IntVec3 tentSize = new IntVec3(5, 0, 5);
        //private IntVec3 campSize = new IntVec3(21, 0, 21);
        private int spacer = 2;
        private List<CampArea> campParts;
        private IntVec3 campCenterSpot;

        // todo move to camp config settings
        private bool hasMedicalTent = true;
        private bool hasStorageTent = true;
        private bool hasProductionTent = true;
        private bool hasAnimalArea = true;
        private bool clearSnow = false;

        private CellRect coordSystem;
        private CellRect campSiteRect;

        private List<Thing> campAssetListRef;

        public CampBuilder(Caravan caravan, Map map)
        {
            this.caravan = caravan;
            this.map = map;

            campParts = new List<CampArea>();
        }

        public bool GenerateCamp(bool tribal = false)
        {
            var stateBackup = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;

            CalculateTentSizes();
            CalculateTentNumbersAndAssignPawnsToTents();
            AssignCampLayout();
            TransformTerrain();
            GenerateBuildings();
            UpdateAreas();
            ApplyZonesAndInventory();

            Current.ProgramState = stateBackup;
            return true;
        }

        protected void CalculateTentSizes()
        {
            // based on colonists, settings and inventory
            // TODO!
        }

        protected void CalculateTentNumbersAndAssignPawnsToTents()
        {
            var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
            var sickColonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist && col.health.hediffSet.HasNaturallyHealingInjury()).ToList();
            var prisoners = caravan.PawnsListForReading.Where(col => col.IsPrisoner).ToList();

            campParts.Add(new CampCenter());
            campParts.Add(new FoodTent());
            // todo change to configurable number including auto (-1)
            if (hasProductionTent) campParts.Add(new ProductionTent());
            if (hasStorageTent) campParts.Add(new StorageTent());
            if (hasMedicalTent && sickColonists.Count == 0) campParts.Add(new MedicalTent());
            if (hasAnimalArea && caravan.PawnsListForReading.Any(pawn => pawn.RaceProps.Animal)) campParts.Add(new AnimalArea());

            List<List<Pawn>> colonistRelationShipPairs = GetRelationShipPairs(colonists);
            colonistRelationShipPairs.ForEach(couple =>
            {
                campParts.Add(new RestTent() { Occupants = new List<Pawn>() { couple[0], couple[1] } });
            });

            if (hasMedicalTent)
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

            colonists.Where(col => !colonistRelationShipPairs
                .SelectMany(pair => pair)
                .Contains(col))
                .ToList()
                .ForEach(col =>
                {
                    var tentWithSpace = campParts?.OfType<RestTent>()?.FirstOrDefault(tent => tent.Occupants.Count < (tentSize.x * tent.CoordSize - 2) && !(tent is MedicalTent) && !(tent is PrisonerTent));
                    if (tentWithSpace == null)
                    {
                        tentWithSpace = new RestTent();
                        campParts.Add(tentWithSpace);
                    }
                    tentWithSpace.Occupants.Add(col);
                });
        }

        protected void AssignCampLayout()
        {
            campCenterSpot = CampHelper.FindCenterCell(map, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
            var campCenter = campParts.OfType<CampCenter>().FirstOrDefault();
            campCenter.Coords.Add(new IntVec3(0, 0, 0));

            var coords = new List<IntVec3>();
            coordSystem = new CellRect(0, 0, 1, 1);

            foreach (var part in campParts)
            {
                if (part is CampCenter) continue;
                part.Coords = FindPlacement(part);
            }

            foreach (var part in campParts)
            {
                part.CellRect = CalculateRect(part);
            }

            campSiteRect = CalcCampSiteRect();
        }

        protected void TransformTerrain()
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

                    var roomRect = CellRect.FromLimits(room.Cells.MinBy(cell => cell.x + cell.z), room.Cells.MaxBy(cell => cell.x + cell.z));
                    foreach (var cell in roomRect.ExpandedBy(1).Cells) map.fogGrid.Unfog(cell);
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

        protected CellRect CalcCampSiteRect()
        {
            var width = campParts.Max(p => p.CellRect.maxX) - campParts.Min(p => p.CellRect.minX);
            var height = campParts.Max(p => p.CellRect.maxZ) - campParts.Min(p => p.CellRect.minZ);
            return new CellRect(campParts.Min(p => p.CellRect.minX) - 1 - spacer,
                campParts.Min(p => p.CellRect.minZ) - 1 - spacer,
                width + (1 + spacer) * 2 + 1,
                height + (1 + spacer) * 2 + 1);
        }

        protected CellRect CalculateRect(CampArea part)
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

        protected List<IntVec3> FindPlacement(CampArea part)
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

            placementCells.ForEach(selected => Log.Message($"Selected: {selected.x} {selected.z} for {part.GetType()}"));
            return placementCells;
        }

        protected List<IntVec3> GetNeigbourCells(IntVec3 cell, IOrderedEnumerable<IntVec3> source, int limit = 0, ForcedTentDirection tentDirection = ForcedTentDirection.None)
        {
            var result = new List<IntVec3>() { cell };
            for (; ; )
            {
                var neighbour = source.FirstOrDefault(cur =>
                    result.Any(res => cur.AdjacentToCardinal(res) && !result.Contains(cur))
                    && result.All(res => tentDirection == ForcedTentDirection.None
                        ? (cur.x == res.x || cur.z == res.z)
                        : tentDirection == ForcedTentDirection.Horizontal
                        ? cur.z == res.z : cur.x == res.x));
                if (neighbour == default || limit != 0 && result.Count == limit) break;
                result.Add(neighbour);
            }

            if (limit != 0 && result.Count < limit) return null;
            return result;
        }

        protected IEnumerable<IntVec3> FindFreeCoords()
        {
            for (int z = coordSystem.Max(cell => cell.z); z >= coordSystem.Min(cell => cell.z); z--)
            {
                for (int x = coordSystem.Min(cell => cell.x); x <= coordSystem.Max(cell => cell.x); x++)
                {
                    if (!campParts.SelectMany(p => p.Coords).Any(c => c.x == x && c.z == z)) yield return new IntVec3(x, 0, z);
                }
            }
        }

        protected List<List<Pawn>> GetRelationShipPairs(List<Pawn> colonists)
        {
            var pairList = new List<List<Pawn>>();
            var prodColList = colonists.ToList();
            for (; ; )
            {
                var selCol = prodColList.FirstOrDefault(col => prodColList.Any(otherCol => otherCol != col && (new[] {
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover),
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse),
                        col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance)}).Contains(otherCol)));

                if (selCol == null) break;
                var otherPawn = LovePartnerRelationUtility.ExistingLovePartner(selCol);
                pairList.Add(new List<Pawn> { selCol, otherPawn });
                prodColList.Remove(selCol);
                prodColList.Remove(otherPawn);
            }

            return pairList;
        }

        protected void GenerateBuildings()
        {
            // todo figure out how best to pass the reference from the comp to the builder
            campAssetListRef = new List<Thing>();
            foreach (var part in campParts)
            {
                part.Build(map, campAssetListRef);
            }

            for (int i = 0; i < campSiteRect.EdgeCells.Count() - 4; i++)
            {
                if (i % 5 != 0) continue;
                var lamp = GenSpawn.Spawn(RimWorld.ThingDefOf.TorchLamp, campSiteRect.EdgeCells.ToArray()[i], map);
                lamp.SetFaction(Faction.OfPlayer);
                var fuelComp = lamp.TryGetComp<CompRefuelable>();
                if (fuelComp != null) fuelComp.allowAutoRefuel = false;
            }

            var center = campParts.FirstOrDefault(part => part is CampCenter) as CampCenter;
            center.Control.TryGetComp<CompCampControl>().CampRects = campParts.Select(part => part.CellRect).ToList();
            center.Control.TryGetComp<CompCampControl>().CampAssets = campAssetListRef;
        }

        protected void UpdateAreas()
        {
            //todo add to settings
            foreach (var cell in campSiteRect)
            {
                map.areaManager.Home[cell] = true;
                // todo add to settings
                if (clearSnow) if (!cell.Roofed(map)) map.areaManager.SnowClear[cell] = true;
            }
        }

        protected void ApplyZonesAndInventory()
        {
            foreach (var zoneTent in campParts.OfType<IZoneTent>())
            {
                zoneTent.CreateZone(map);
                zoneTent.ApplyInventory(map, caravan);
            }

            foreach (var shelfTent in campParts.OfType<IShelfTent>())
            {
                shelfTent.FillShelfs(map, caravan);
            }

            foreach (var areaRestriction in campParts.OfType<IAreaRestrictionTent>())
            {
                areaRestriction.CreateNewRestrictionArea(map, caravan);
                areaRestriction.AssignPawnsToAreas(map, caravan);
            }

            CampHelper.AddAnimalFreeAreaRestriction(campParts.OfType<IZoneTent>().Where(part => part is FoodTent), map);
        }

    }
}
