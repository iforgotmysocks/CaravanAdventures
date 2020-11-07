using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

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

        public static Thing GetFirstOrderedThingOfCategoryFromCaravan(Caravan caravan, ThingCategoryDef[] validCategories, ThingDef[] unvalidThings = null)
        {
            Thing selected = null;
            foreach (var category in validCategories)
            {
                selected = caravan.AllThings.Where(x => x?.def?.thingCategories != null 
                    && x.def.thingCategories.Contains(category) 
                    && ((unvalidThings == null || unvalidThings.Length == 0) || Array.IndexOf(unvalidThings, x.def) == -1))
                    .OrderByDescending(x => x.stackCount).FirstOrDefault();

                if (selected != null) return selected;
            }
            return selected;
        }

        internal static void AddAnimalFreeAreaRestriction(IEnumerable<IZoneTent> parts, Map map)
        {
            var animalArea = new Area_Allowed(map.areaManager);
            map.areaManager.AllAreas.Add(animalArea);
            animalArea.SetLabel("CAAnimalNoFoodAreaLabel".Translate());
            map.AllCells.ToList().ForEach(cell => animalArea[cell] = true);
            parts.Select(part => part.GetZone()).ToList().ForEach(zone => zone.Cells.ForEach(cell => animalArea[cell] = false));
            animalArea.AreaUpdate();
        }
    }
}
