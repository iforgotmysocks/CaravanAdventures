using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using UnityEngine;

namespace CaravanAdventures.CaravanStory
{
    class GenStep_ScatterMasterShrines : GenStep_ScatterRuinsSimple
	{
		private static int calcSize = 0;
		private static int calcSize2 = 0;
		private static readonly IntRange SizeRange = new IntRange(100, 120); // 60, 80

		public override int SeedPart
		{
			get
			{
				return 1804522483;
			}
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!CanScatterAtAdjusted(c, map))
			{
				return false;
			}

			return true;
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			calcSize = SizeRangeDependingOnMapSize(true).RandomInRange;
			calcSize2 = SizeRangeDependingOnMapSize(true).RandomInRange;
			return base.TryFindScatterCell(map, out result);
		}

		public static bool CanScatterAtAdjusted(IntVec3 c, Map map)
		{
			if (!SupportsAdjustedStructureType(c, map, TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			CellRect rect = new CellRect(c.x, c.z, calcSize, calcSize2).ClipInsideMap(map);
			return CanPlaceAncientBuildingInRangeAfterAdjustingGround(rect, map, true);
		}

		public static bool SupportsAdjustedStructureType(IntVec3 c, Map map, TerrainAffordanceDef surfaceType)
		{
			var terrain = c.GetTerrain(map);
			if (terrain.affordances.Contains(surfaceType)) return true;
			else if (terrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) || terrain.affordances.Contains(TerrainAffordanceDefOf.Diggable))
			{
				//map.terrainGrid.SetTerrain(c, TerrainDefOf.FlagstoneSandstone);
				return true;
			}
			else
			{
				DLog.Message($"Failing affordance");
				return false;
			}
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
		{
			CellRect rect = new CellRect(loc.x, loc.z, calcSize, calcSize2);
			rect.ClipInsideMap(map);
			if (rect.Width != calcSize || rect.Height != calcSize2)
			{
				DLog.Message($"Scattering failed due to not fitting sizes");
                return;
            }
			foreach (IntVec3 c in rect.Cells)
			{
				List<Thing> list = map.thingGrid.ThingsListAt(c);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
					{
						DLog.Message($"Canceling because of existing shrine");
						return;
					}
				}
			}
			// todo - CanPlaceAncientBuildingInRangeAfterAdjustingGround() see if we cant make that work?
			//if (!base.CanPlaceAncientBuildingInRange(rect, map))
			if (!CanPlaceAncientBuildingInRangeAfterAdjustingGround(rect, map))
			{
				DLog.Message($"CanPlaceAncientBuildingInRangeAfterAdjustingGround");
				return;
			}
			ResolveParams resolveParams = default;
			resolveParams.rect = rect;
			resolveParams.disableSinglePawn = new bool?(true);
			resolveParams.disableHives = true;
			resolveParams.makeWarningLetter = new bool?(true);
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("ancientTemple", resolveParams, null);
			BaseGen.Generate();
		}


		private static IntRange SizeRangeDependingOnMapSize(bool overrideWithSetSize = false)
		{
			if (overrideWithSetSize) return SizeRange;
			var mapSize = Find.World.info.initialMapSize;
			DLog.Message($"map size: {mapSize}");
			var sizeValue = mapSize.x * mapSize.z;
			var endSize = Convert.ToInt32(Math.Round(sizeValue * 0.0008, 0));
			var offset = Convert.ToInt32(Math.Round(sizeValue * 0.00008, 0));
			DLog.Message($"calc size: {endSize - offset} / {endSize + offset}  -   base: {endSize} offset: {offset}");
			return new IntRange(endSize - offset, endSize + offset);
		}

		public static bool CanPlaceAncientBuildingInRangeAfterAdjustingGround(CellRect rect, Map map, bool fakeTerrainReplacement = false)
		{
			var newTerrainCells = new List<IntVec3>();
			foreach (IntVec3 c in rect.Cells)
			{
				if (c.InBounds(map))
				{
					TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
					//if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
					if (terrainDef.HasTag("River"))
					{
						return false;
					}
					if (!CanBuildOnAdjustedTerrain(ThingDefOf.Wall, c, map, Rot4.North, newTerrainCells, null, null))
					{
						DLog.Message($"Canceling due to terrain");
						return false;
					}
				}
			}

			if (!fakeTerrainReplacement)
			{
				foreach (var terrain in newTerrainCells)
				{
					//map.terrainGrid.SetTerrain(terrain, GenStep_RocksFromGrid.RockDefAt(terrain).building.naturalTerrain);
					var newTerrain = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, 0.7f);
					if (newTerrain != TerrainDefOf.Soil) DLog.Warning($"Creating new terrain: {newTerrain.defName}");
					map.terrainGrid.SetTerrain(terrain, newTerrain);
				}
			}
			
			return true;
		}

		public static bool CanBuildOnAdjustedTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, List<IntVec3> newTerrainCells, Thing thingToIgnore = null, ThingDef stuffDef = null)
		{
			if (entDef is TerrainDef && !c.GetTerrain(map).changeable)
			{
				DLog.Message($"Terrain not changeable: {c.GetTerrain(map).defName}");
				return false;
			}
			// todo collect failing cells and replace terrain afterwards
			TerrainAffordanceDef terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
			if (terrainAffordanceNeed != null)
			{
				CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
				cellRect.ClipInsideMap(map);
				foreach (IntVec3 c2 in cellRect)
				{
					if (!map.terrainGrid.TerrainAt(c2).affordances.Contains(terrainAffordanceNeed))
					{
						if (map.terrainGrid.TerrainAt(c2).affordances.Contains(TerrainAffordanceDefOf.Bridgeable) || map.terrainGrid.TerrainAt(c2).affordances.Contains(TerrainAffordanceDefOf.Diggable)) newTerrainCells.Add(c2);
						else
						{
							DLog.Message($"Failing affordance for terrain def: {map.terrainGrid.TerrainAt(c2).defName}");
							return false;
						}
					}
					List<Thing> thingList = c2.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i] != thingToIgnore)
						{
							TerrainDef terrainDef = thingList[i].def.entityDefToBuild as TerrainDef;
							if (terrainDef != null && !terrainDef.affordances.Contains(terrainAffordanceNeed))
							{
								if (!terrainDef.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) && !terrainDef.affordances.Contains(TerrainAffordanceDefOf.Diggable))
								{
									DLog.Message($"thinglist affordinace failed");
									return false;
								}
								//return false;
							}
						}
					}
				}

			
				return true;
			}
			return true;
		}

	}
}
