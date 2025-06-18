using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class GenStep_ScatterSmallShrines : GenStep_ScatterShrines // GenStep_ScatterRuinsSimple
    {
        public override int SeedPart
        {
            get => 1831232483;
        }

        private int usedRectsPadding = 2;

        protected override bool ShouldSkipMap(Map map) => false;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            //if (!c.UsesOutdoorTemperature(map)) return false;
            if (!CanPlaceAncientBuildingInRange(EffectiveRectAt(c).ClipInsideMap(map), map))
            {
                return false;
            }
            if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var))
            {
                CellRect cellRect = EffectiveRectAt(c);
                foreach (CellRect item in var)
                {
                    if (cellRect.Overlaps(item.ExpandedBy(usedRectsPadding)))
                    {
                        return false;
                    }
                }
            }

            if (!GenStep_ScatterMasterShrines.CanScatterAtAdjusted(c, map))
            {
                return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
        {
            List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
            CellRect cellRect = EffectiveRectAt(loc);
            CellRect cellRect2 = cellRect.ClipInsideMap(map);
            if (cellRect2.Width != cellRect.Width || cellRect2.Height != cellRect.Height)
            {
                return;
            }
            foreach (IntVec3 cell in cellRect.Cells)
            {
                List<Thing> list = map.thingGrid.ThingsListAt(cell);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
                    {
                        return;
                    }
                }
            }
            if (orGenerateVar.Contains(cellRect) || !GenStep_ScatterMasterShrines.CanPlaceAncientBuildingInRangeAfterAdjustingGround(cellRect, map))
            {
                return;
            }
            orGenerateVar.Add(cellRect);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = cellRect;
            resolveParams.disableSinglePawn = true;
            resolveParams.disableHives = true;
            resolveParams.makeWarningLetter = false;
            resolveParams.fleshbeastsCount = 0;
            if (ModSettings.storyMode == StoryMode.Performance) resolveParams.podContentsType = PodContentsType.Empty;
            else if (Rand.Chance(0.5f)) resolveParams.podContentsType = PodContentsType.Empty;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("ancientTemple", resolveParams);
            BaseGen.Generate();
        }

    }
}
