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
	// Find fabric and leather used to build buildings
	// find rect in middle of map large enough to fit tents for x pawns
	// build bridges on non compatible ground
	// generate buildings 
	// create small versions of generators etc 
	// buildings:
	// - small generator
	// - small one way climataion unit (a/c & heat, requires some fuel)

	// functionality: 
	// - add right click option to campfire that turns all camping stuff back into resources

	// add another field rect

	class CampBuilder
    {
		private Map map;
		private Caravan caravan;

		private IntVec3 tentSize = new IntVec3(5, 0, 5);
		//private IntVec3 campSize = new IntVec3(21, 0, 21);
		private int spacer = 1;
		private List<CampArea> campParts;
		private IntVec3 campCenterSpot;

		// todo move to camp config settings
		private bool hasMedicalTent = true;
		private bool hasStorageTent = true;
		private bool hasProductionTent = false;

		private CellRect coordSystem;
		private CellRect campSiteRect;

		public CampBuilder(Caravan caravan, Map map)
        {
			this.caravan = caravan;
			this.map = map;

			campParts = new List<CampArea>();
        }

        public bool GenerateCamp(bool tribal = false)
        {
			CalculateTentNumbersAndAssignPawnsToTents();
			AssignCampLayout();
            TransformTerrain();
            GenerateBuildings();
            return true;
        }

        public void CalculateTentNumbersAndAssignPawnsToTents()
		{
			var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
			var prisoners = caravan.PawnsListForReading.Where(col => col.IsPrisoner).ToList();

			campParts.Add(new CampCenter());
			campParts.Add(new FoodTent());
			// todo change to configurable number including auto (-1)
			if (hasMedicalTent) campParts.Add(new MedicalTent());
			if (hasProductionTent) campParts.Add(new ProductionTent());
			if (hasStorageTent) campParts.Add(new StorageTent());

			List<List<Pawn>> colonistRelationShipPairs = GetRelationShipPairs(colonists, hasMedicalTent);
			colonistRelationShipPairs.ForEach(couple =>
			{
				campParts.Add(new RestTent() { Occupants = new List<Pawn>() { couple[0], couple[1] } });
			});

			colonists.Where(col => !colonistRelationShipPairs
				.SelectMany(pair => pair)
				.Contains(col))
				.ToList()
				.ForEach(col =>
				{
					var tentWithSpace = campParts?.OfType<RestTent>()?.FirstOrDefault(tent => tent.Occupants.Count < 3 && !(tent is MedicalTent));
					if (tentWithSpace == null)
					{
						tentWithSpace = new RestTent();
						campParts.Add(tentWithSpace);
					}
					tentWithSpace.Occupants.Add(col);
				});

			// todo prisoners
		}

		private void AssignCampLayout()
		{
			// todo new steps 
			// -> Create base rect around center spot
			// -> check if still space in current rect, if not increase rect size to second level all around 1 -> 9 -> 27
			// -> check vertical and horizontal first, then fill empty rect places around

			campCenterSpot = CampHelper.FindCenterCell(map, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
			var campCenter = campParts.OfType<CampCenter>().FirstOrDefault();
			campCenter.Coords.Add(new IntVec3(0, 0, 0));

			var coords = new List<IntVec3>();

			coordSystem = new CellRect(0, 0, 1, 1);
			//campParts.ForEach(campPart => size += campPart.CoordSize);
			//while (coordSystem.Cells.Count() < size) coordSystem.ExpandedBy(1);

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

		private void TransformTerrain()
		{
			var stateBackup = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;

			foreach (var c in campSiteRect.Cells)
			{
				foreach (var thing in map.thingGrid.ThingsListAt(c).Reverse<Thing>()) thing.Destroy();
				map.roofGrid.SetRoof(c, null);
				map.fogGrid.Unfog(c);
				var terrain = map.terrainGrid.TerrainAt(c);
				if (!terrain.affordances.Any(x => (new[] { TerrainAffordanceDefOf.Bridgeable, TerrainAffordanceDefOf.Diggable, TerrainAffordanceDefOf.Light }).Contains(x))) map.terrainGrid.SetTerrain(c, TerrainDefOf.Gravel);
				else if (terrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) && !terrain.affordances.Contains(TerrainAffordanceDefOf.Light)) map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
			}
			campSiteRect.ExpandedBy(1).EdgeCells.ToList().ForEach(cell => map.fogGrid.Unfog(cell));

			Current.ProgramState = stateBackup;
		}

        private CellRect CalcCampSiteRect()
		{
			var width = campParts.Max(p => p.CellRect.maxX) - campParts.Min(p => p.CellRect.minX);
			var height = campParts.Max(p => p.CellRect.maxZ) - campParts.Min(p => p.CellRect.minZ);
			return new CellRect(campParts.Min(p => p.CellRect.minX) - 1 - spacer, 
				campParts.Min(p => p.CellRect.minZ) - 1 - spacer, 
				width + (1 + spacer) * 2 + 1, 
				height + (1 + spacer) * 2 + 1);
        }

        private CellRect CalculateRect(CampArea part)
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

        private List<IntVec3> FindPlacement(CampArea part)
		{
			var placementCells = new List<IntVec3>();
			var center = new IntVec3(0, 0, 0);
			if (FindFreeCoords().Count() == 0) coordSystem = coordSystem.ExpandedBy(1);
			var free = FindFreeCoords().OrderBy(coord => coord.DistanceTo(center));
			// todo add forced sides to the x and y selection in GetNeighbourCells for specific building types
			if (part.CoordSize > 1)
			{
				for (; ;)
                {
					foreach (var cell in free)
					{
						var cells = GetNeigbourCells(cell, free, part.CoordSize, part.ForcedTentDirection); // add part.direction
						if (cells != null) placementCells = cells;
						break;
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

		public List<IntVec3> GetNeigbourCells(IntVec3 cell, IOrderedEnumerable<IntVec3> source, int limit = 0, ForcedTentDirection tentDirection = ForcedTentDirection.None)
        {
			var result = new List<IntVec3>() { cell };
			for(; ;)
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

        private IEnumerable<IntVec3> FindFreeCoords()
        {
            for (int z = coordSystem.Max(cell => cell.z); z >= coordSystem.Min(cell => cell.z); z--)
            {
                for (int x = coordSystem.Min(cell => cell.x); x <= coordSystem.Max(cell => cell.x); x++)
                {
					if (!campParts.SelectMany(p => p.Coords).Any(c => c.x == x && c.z == z)) yield return new IntVec3(x, 0, z);
				}
            }
        }

        private List<List<Pawn>> GetRelationShipPairs(List<Pawn> colonists, bool skipInjured)
        {
			var pairList = new List<List<Pawn>>();
			var prodColList = colonists.ToList();
			for(; ;)
            {
				var selCol = prodColList.FirstOrDefault(col => (skipInjured ? !col.health.hediffSet.HasNaturallyHealingInjury() : true) 
					&& prodColList.Any(otherCol => otherCol != col && (new[] { 
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

        public void GenerateBuildings()
        {
            var stateBackup = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;

			foreach (var part in campParts)
			{
				part.Build(map);
			}

            //foreach (var c in campSiteRect.EdgeCells)
            //{
            //    GenSpawn.Spawn(RimWorld.ThingDefOf.TorchLamp, c, map);
            //}

            for (int i = 0; i < campSiteRect.EdgeCells.Count() - 4; i++)
            {
				if (i % 5 == 0) GenSpawn.Spawn(RimWorld.ThingDefOf.TorchLamp, campSiteRect.EdgeCells.ToArray()[i], map);
			}

            Current.ProgramState = stateBackup;
        }

		public void GenerateFoodTent()
        {
			// todo add eatable foods and when non tribal, cool
        }


    }
}
