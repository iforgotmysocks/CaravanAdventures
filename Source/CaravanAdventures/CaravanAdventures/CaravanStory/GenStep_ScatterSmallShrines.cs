using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class GenStep_ScatterSmallShrines : GenStep_ScatterShrines
	{
		public override int SeedPart
		{
			get
			{
				return 1841232483;
			}
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
            //if (!base.CanScatterAt(c, map))
            //{
            //	return false;
            //}
            if (!c.UsesOutdoorTemperature(map)) return false;
            if (!GenStep_ScatterMasterShrines.CanScatterAtAdjusted(c, map))
            {
                return false;
            }
			return true;
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
            if (!GenStep_ScatterMasterShrines.CanPlaceAncientBuildingInRangeAfterAdjustingGround(rect, map))
            {
                return;
            }
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.disableSinglePawn = new bool?(true);
            resolveParams.disableHives = new bool?(true);
            resolveParams.makeWarningLetter = new bool?(false);
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("ancientTemple", resolveParams, null);
            BaseGen.Generate();
        }

        private static readonly IntRange SizeRange = new IntRange(15, 20);
    }
}
