using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class SlaveTent : RestTent
    {
        public SlaveTent()
        {
            this.CoordSize = 1;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            CheckAndPostApplyBedState(map, BedOwnerType.Slave);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);
            CheckAndPostApplyBedState(map, BedOwnerType.Slave);
        }

        
    }
}
