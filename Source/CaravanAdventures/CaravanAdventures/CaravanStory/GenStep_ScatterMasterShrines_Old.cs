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
	// todo - delete whole class when done with the new version
    class GenStep_ScatterMasterShrines_Old : GenStep_ScatterRuinsSimple
	{
		public override int SeedPart
		{
			get
			{
				return 1804522483;
			}
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
		{
			int randomInRange = SizeRange.RandomInRange;
			int randomInRange2 = SizeRange.RandomInRange;
			CellRect rect = new CellRect(loc.x, loc.z, randomInRange, randomInRange2);
			rect.ClipInsideMap(map);
			if (rect.Width != randomInRange || rect.Height != randomInRange2)
			{
				return;
			}
			foreach (IntVec3 c in rect.Cells)
			{
				List<Thing> list = map.thingGrid.ThingsListAt(c);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
					{
						return;
					}
				}
			}
			if (!base.CanPlaceAncientBuildingInRange(rect, map))
			//if (!CanPlaceAncientBuildingInRangeAfterAdjustingGround(rect, map))
			{
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

		private static readonly IntRange SizeRange = new IntRange(60, 80);

		private static IntRange SizeRangeDependingOnMapSize()
		{
			var mapSize = Find.World.info.initialMapSize;
			Log.Message($"map size: {mapSize}");
			var sizeValue = mapSize.x * mapSize.z;
			var endSize = Convert.ToInt32(Math.Round(sizeValue * 0.0008, 0));
			var offset = Convert.ToInt32(Math.Round(sizeValue * 0.00008, 0));
			Log.Message($"calc size: {endSize - offset} / {endSize + offset}  -   base: {endSize} offset: {offset}");
			return new IntRange(endSize - offset, endSize + offset);
		}

		private static bool CanPlaceAncientBuildingInRangeAfterAdjustingGround(CellRect rect, Map map)
		{
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
					if (!CanBuildOnAdjustedTerrain(ThingDefOf.Wall, c, map, Rot4.North, null, null))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static bool CanBuildOnAdjustedTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null, ThingDef stuffDef = null)
		{
			if (entDef is TerrainDef && !c.GetTerrain(map).changeable)
			{
				return false;
			}
			TerrainAffordanceDef terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
			if (terrainAffordanceNeed != null)
			{
				CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
				cellRect.ClipInsideMap(map);
				foreach (IntVec3 c2 in cellRect)
				{
					if (!map.terrainGrid.TerrainAt(c2).affordances.Contains(terrainAffordanceNeed))
					{
						if (map.terrainGrid.TerrainAt(c2).affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) map.terrainGrid.SetTerrain(c2, TerrainDefOf.Concrete);
						else return false;
					}
					List<Thing> thingList = c2.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i] != thingToIgnore)
						{
							TerrainDef terrainDef = thingList[i].def.entityDefToBuild as TerrainDef;
							if (terrainDef != null && !terrainDef.affordances.Contains(terrainAffordanceNeed))
							{
								return false;
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
