using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using CaravanAdventures.Patches;

namespace CaravanAdventures.CaravanItemSelection
{
    static class FilterHelper
    {
        public static bool DoFiltersApply<T>(FilterSet packUp, T trans)
        {
            Type t = trans.GetType();
            var thingDef = t.GetProperty("ThingDef")?.GetValue(trans, null) as ThingDef;
            if (thingDef == null) return false;

            Log.Message($"Got here on {thingDef.defName}");

            bool[] filterResults = null;

            List<object> protectedFromExclusion = new List<object>();

            foreach (var filter in packUp.appliedFilters.OrderBy(x => x.Operation))
            {
                if (filter.Connection == FilterConnection.OR)
                {
                    if (filter.Operation == FilterOperation.Include)
                    {
                        if (filter.ThingDefs.Contains(thingDef)) return true;
                        if (thingDef.thingCategories?.Count == null || thingDef.thingCategories.Count == 0)
                        {
                            Log.Message($"{thingDef.defName} has no thingCategories.");
                            continue;
                        }
                        if (thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return true;
                    }
                    else if (filter.Operation == FilterOperation.Exclude)
                    {
                        if (filter.ThingDefs.Contains(thingDef)) return false;
                        if (thingDef.thingCategories?.Count == null || thingDef.thingCategories.Count == 0)
                        {
                            Log.Message($"{thingDef.defName} has no thingCategories.");
                            continue;
                        }
                        if (thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return false;
                    }

                }
            }
            return true;
        }

        public static void SetMaxAmount<T>(T trans)
        {
            var maximumToTransferAmount = trans.GetType().GetMethod("GetMaximumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { maximumToTransferAmount });
        }

        public static void SetMinAmount<T>(T trans)
        {
            var minimumToTransferAmount = trans.GetType().GetMethod("GetMinimumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { minimumToTransferAmount });
        }
    }
}
