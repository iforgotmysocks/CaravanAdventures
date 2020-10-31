using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class ProductionTent : Tent
    {
        public ProductionTent()
        {
            CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = 4;
        }

        public override void Build(Map map)
        {
            base.Build(map);


        }
    }
}
