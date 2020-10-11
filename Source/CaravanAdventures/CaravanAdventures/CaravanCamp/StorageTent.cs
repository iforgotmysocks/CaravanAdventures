using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class StorageTent : Tent
    {
        public StorageTent()
        {
            this.CoordSize = 2;
        }

        public override void Build(Map map)
        {
            base.Build(map);
        }
    }
}
