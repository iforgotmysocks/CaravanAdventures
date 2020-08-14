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
			int randomInRange = GenStep_ScatterMasterShrines.SizeRange.RandomInRange;
			int randomInRange2 = GenStep_ScatterMasterShrines.SizeRange.RandomInRange;
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
	}
}
