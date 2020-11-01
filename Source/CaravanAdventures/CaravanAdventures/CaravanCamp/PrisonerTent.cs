using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class PrisonerTent : RestTent
    {
        public PrisonerTent()
        {
            this.CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            // todo maybe just add a bed list to parent
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.ForPrisoners = true;
            }
        }
    }
}
