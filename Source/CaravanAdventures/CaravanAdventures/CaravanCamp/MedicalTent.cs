using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class MedicalTent : RestTent
    {
        public MedicalTent()
        {
            this.CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
        }

        public override void Build(Map map)
        {
            base.Build(map);
            // todo maybe just add a bed list to parent
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null) continue;
                bed.Medical = true;
            }
        }
    }
}
