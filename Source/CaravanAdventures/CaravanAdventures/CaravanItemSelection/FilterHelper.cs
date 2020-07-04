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
        public static bool DoFiltersApply<T>(FilterSet filterSet, T trans)
        {
            Type t = trans.GetType();
            var thingDef = t.GetProperty("ThingDef")?.GetValue(trans, null) as ThingDef;
            if (thingDef == null) return false;
            var thing = t.GetProperty("AnyThing")?.GetValue(trans, null) as Thing;

            var standartResult = true;

            Log.Message($"Got here on {thingDef.defName}");
            if (thingDef.thingCategories != null)
            {
                foreach (var cat in thingDef.thingCategories)
                {
                    Log.Message($"Cat: {cat.defName}");
                }
            }
            
            bool[] filterResults = null;

            foreach (var filter in filterSet.appliedFilters.OrderBy(x => x.Operation))
            {
                var canUseQuality = false;
                QualityCategory quality = QualityCategory.Awful;
                if (filter.MaxQuality != null)
                {
                    if (thing != null && thing.TryGetQuality(out quality)) canUseQuality = true;
                    Log.Message($"Did the conversion work? {canUseQuality} :  {quality}");
                }
                if (filter.Connection == FilterConnection.NEG)
                {
                    standartResult = !standartResult;
                }
                if (filter.Operation == FilterOperation.Include)
                {
                    if (filter.ThingDefs.Contains(thingDef)) return true;
                    if (thingDef.thingCategories?.Count == null || thingDef.thingCategories.Count == 0)
                    {
                        Log.Message($"{thingDef.defName} has no thingCategories.");
                        continue;
                    }
                    if (canUseQuality)
                    {
                        Log.Message($"Can use quality on {thingDef.defName}: {canUseQuality}");
                        if (((int)quality <= (int)filter.MaxQuality) && thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return true;
                    }
                    else if (thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return true;
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
            return standartResult;
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

        internal static void SetAmount(TransferableOneWay trans, int amount)
        {
            var maximumToTransferAmount = trans.GetType().GetMethod("GetMaximumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { amount <= (int)maximumToTransferAmount ? amount : maximumToTransferAmount});

        }
    }
}
