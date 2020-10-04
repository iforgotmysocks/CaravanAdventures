using RimWorld;
using RimWorld.Planet;
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
		private IntVec3 campSize = new IntVec3(21, 0, 21);
		private int spacer = 1;
		private List<CampArea> campParts;

		// todo move to camp config settings
		private bool hasMedicalTent = false;
		private bool hasStorageTent = false;
		private bool hasProductionTent = false;

		private CellRect campSiteRect;

		public CampBuilder(Caravan caravan, Map map)
        {
			this.caravan = caravan;
			this.map = map;

			campParts = new List<CampArea>();
        }

        public bool GenerateCamp(bool tribal = false)
        {
			CalculateCampSizeAndAssignPawnsToTents();
			AssignCampLayout();
			TransformTerrain();
			
			GenerateBuildings();


			/*
             Sketch sketch = SketchGen.Generate(SketchResolverDefOf.Monument, resolveParams);
			sketch.Spawn(map, rp.rect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, true, true, null, false, true, null, null);
			CellRect rect = SketchGenUtility.FindBiggestRect(sketch, delegate(IntVec3 x)
			{
				if (sketch.TerrainAt(x) != null)
				{
					return !sketch.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall);
				}
				return false;
			}).MovedBy(rp.rect.CenterCell);
			for (int i = 0; i < sketch.Things.Count; i++)
			{
				if (sketch.Things[i].def == ThingDefOf.Wall)
				{
					IntVec3 intVec = sketch.Things[i].pos + rp.rect.CenterCell;
					if (cellRect.IsEmpty)
					{
						cellRect = CellRect.SingleCell(intVec);
					}
					else
					{
						cellRect = CellRect.FromLimits(Mathf.Min(cellRect.minX, intVec.x), Mathf.Min(cellRect.minZ, intVec.z), Mathf.Max(cellRect.maxX, intVec.x), Mathf.Max(cellRect.maxZ, intVec.z));
					}
				}
			} 
              
              
            */

			return true;
        }

        private void TransformTerrain()
        {
			var stateBackup = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;

			foreach (var c in campSiteRect.Cells)
            {
				foreach (var thing in map.thingGrid.ThingsListAt(c).Reverse<Thing>()) thing.Destroy(); 
				map.roofGrid.SetRoof(c, null);
				var terrain = map.terrainGrid.TerrainAt(c);
				if (!terrain.affordances.Any(x => (new[] { TerrainAffordanceDefOf.Bridgeable, TerrainAffordanceDefOf.Diggable, TerrainAffordanceDefOf.Light }).Contains(x))) map.terrainGrid.SetTerrain(c, TerrainDefOf.Gravel);
				else if (terrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) && !terrain.affordances.Contains(TerrainAffordanceDefOf.Light)) map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
            }

			Current.ProgramState = stateBackup;
		}

        public void CalculateCampSizeAndAssignPawnsToTents()
		{
			// tent size: 
			// xxxx
			// xbbx
			// xbbx
			// xflx
			// xexx

			// storage tent ? 
			// food tent 
			// medical tent
			// production tent
			// prison tent
			var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
			var prisoners = caravan.PawnsListForReading.Where(col => col.IsPrisoner).ToList();

			if (hasMedicalTent) campParts.Add(new MedicalTent());
			if (hasProductionTent) campParts.Add(new ProductionTent());
			if (hasStorageTent) campParts.Add(new EmptyTent());

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
					var tentWithSpace = campParts?.OfType<RestTent>()?.FirstOrDefault(tent => tent.Occupants.Count < 3);
					if (tentWithSpace == null)
					{
						tentWithSpace = new RestTent();
						campParts.Add(tentWithSpace);
					}
					tentWithSpace.Occupants.Add(col);
				});

			// todo prisoners

			var tentCount = campParts.Count + 1;
			var baseDevider = 3;
			for (; ;)
            {
				if (tentCount / baseDevider <= baseDevider) break;
				baseDevider++;
			}
			var tentXCount = baseDevider;
			var tentYCount = baseDevider;

			if (baseDevider > 3 && (tentCount * baseDevider) - (tentCount * (baseDevider - 1)) <= baseDevider * (baseDevider - 1))
            {
				tentYCount--;
            }

			// 2 * 1 is fence placeholder
			var sizeX = (tentXCount * spacer + spacer + 2 * 1) + (tentXCount * tentSize.x);
			var sizeY = (tentYCount * spacer + spacer + 2 * 1) + (tentYCount * tentSize.x);

			var center = CampHelper.FindCenterCell(map, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
			campSiteRect = CellRect.CenteredOn(center, sizeX, sizeY);

		}
		private void AssignCampLayout()
		{
			// todo new steps 
			// -> Create base rect around center spot
			// -> check if still space in current rect, if not increase rect size to second level all around 1 -> 9 -> 27
			// -> check vertical and horizontal first, then fill empty rect places around

            for (int i = 0; i < (campParts.Count >= 9 ? campParts.Count : 9); i++)
            {

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
						col.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse) }).Contains(otherCol)));

				if (selCol == null) break;
				var otherPawn = selCol.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover) ?? selCol.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
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

			foreach (var c in campSiteRect)
            {
				GenSpawn.Spawn(ThingDefOf.TorchLamp, c, map);
            }


            Current.ProgramState = stateBackup;
        }

		public void GenerateFoodTent()
        {
			// todo add eatable foods and when non tribal, cool
        }


    }
}
