using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CaravanAdventures.CaravanCamp
{
    class CampHelper
    {

        public static IntVec3 FindCenterCell(Map map, Predicate<IntVec3> extraCellValidator)
        {
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false);
            Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParms);
            IntVec3 result;
            if (extraCellValidator != null && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => baseValidator(x) && extraCellValidator(x), map, out result))
            {
                return result;
            }
            if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(baseValidator, map, out result))
            {
                return result;
            }
            Log.Warning("Could not find any valid cell.", false);
            return CellFinder.RandomCell(map);
        }
    }
}
