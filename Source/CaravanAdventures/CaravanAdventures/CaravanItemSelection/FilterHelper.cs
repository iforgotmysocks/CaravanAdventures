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
        internal static bool DoFiltersApply<T>(FilterSet filterSet, T trans)
        {
            Type t = trans.GetType();
            var thingDef = t.GetProperty("ThingDef")?.GetValue(trans, null) as ThingDef;
            if (thingDef == null) return false;
            var thing = t.GetProperty("AnyThing")?.GetValue(trans, null) as Thing;
            var standartResult = true;

            var minifiedThing = thing as MinifiedThing;
            if (minifiedThing != null)
            {
                thing = minifiedThing.InnerThing;
                thingDef = thing.def;
            }

            foreach (var filter in filterSet.appliedFilters.OrderBy(x => x.Operation))
            {
                var canUseQuality = false;
                QualityCategory quality = QualityCategory.Awful;
                if (filter.MaxQuality != null)
                {
                    if (thing != null && thing.TryGetQuality(out quality)) canUseQuality = true;
                }
                if (filter.Connection == FilterConnection.NEG)
                {
                    standartResult = !standartResult;
                }
                if (filter.Operation == FilterOperation.Include)
                {
                    if (thing is Apparel apparel && apparel.WornByCorpse) return false;
                    var biocoded = thing.TryGetComp<CompBiocodable>();
                    if (biocoded?.Biocoded ?? false) return false;
                    if (filter.ThingDefs.Contains(thingDef)) return true;
                    if (thingDef.thingCategories?.Count == null || thingDef.thingCategories.Count == 0)
                    {
                        Log.Message($"{thingDef.defName} has no thingCategories.");
                        continue;
                    }
                    if (canUseQuality)
                    {
                        if (((int)quality <= (int)filter.MaxQuality) && thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return true;
                    }
                    else if (thingDef.thingCategories.Any(x => filter.ThingCategoryDefs.Contains(x))) return true;
                }
                else if (filter.Operation == FilterOperation.Exclude)
                {
                    if (thing is Apparel apparel && apparel.WornByCorpse) return false;
                    var biocoded = thing.TryGetComp<CompBiocodable>();
                    if (biocoded?.Biocoded ?? false) return false;
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
        internal static void ApplyTravelSupplies(List<Patches.Section> sections, FilterSet journey, List<Pawn> caravanMembers)
        {
            List<object[]> requiredSupplyStatus = new List<object[]>();
            var restAmount = 0;
            foreach (var filter in journey.appliedFilters)
            {
                restAmount = filter.Amount * (caravanMembers != null ? caravanMembers.Count : 0);
                if (filter.ThingDefs.Contains(ThingDefOf.Bedroll)) filter.ThingDefs.Add(ThingDefOf.MinifiedThing);
                var items = sections.SelectMany(section => section.cachedTransferables.Where(trans => filter.ThingDefs.Contains(trans.ThingDef)).Select(trans => trans));
                foreach (var item in items.OrderByDescending(trans => trans.AnyThing.GetInnerIfMinified().MarketValue))
                {
                    if (ThingIsMinifiedButNotBedroll(item)) continue;
                    restAmount -= item.CountToTransfer;
                    var canFillBy = item.GetMaximumToTransfer() - item.CountToTransfer;
                    if (canFillBy >= restAmount) {
                        item.AdjustBy(restAmount);
                        restAmount = 0;
                    }
                    else
                    {
                        item.AdjustBy(canFillBy);
                        restAmount -= canFillBy;
                    }
                }
            }
        }

        private static bool ThingIsMinifiedButNotBedroll(TransferableOneWay trans)
        {
            if (trans.ThingDef != ThingDefOf.MinifiedThing) return false;
            var unminifiedThing = MinifyUtility.GetInnerIfMinified(trans.AnyThing);
            if (unminifiedThing.def == ThingDefOf.Bedroll) return false;
            return true;
        }

        internal static void SetMaxAmount<T>(T trans)
        {
            var maximumToTransferAmount = trans.GetType().GetMethod("GetMaximumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { maximumToTransferAmount });
        }

        internal static void SetMinAmount<T>(T trans)
        {
            var minimumToTransferAmount = trans.GetType().GetMethod("GetMinimumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { minimumToTransferAmount });
        }

        internal static void SetAmount<T>(T trans, int amount)
        {
            var maximumToTransferAmount = trans.GetType().GetMethod("GetMaximumToTransfer").Invoke(trans, null);
            trans.GetType().GetMethod("AdjustTo").Invoke(trans, new object[] { amount <= (int)maximumToTransferAmount ? amount : maximumToTransferAmount});

        }
    
    }
}
