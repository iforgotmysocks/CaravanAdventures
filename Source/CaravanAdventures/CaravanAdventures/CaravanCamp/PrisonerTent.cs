﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class PrisonerTent : RestTent
    {
        public PrisonerTent()
        {
            this.CoordSize = 1;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.ForPrisoners = true;
            }
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.ForPrisoners = true;
            }
        }
    }
}
