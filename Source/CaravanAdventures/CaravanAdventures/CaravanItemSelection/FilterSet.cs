﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanItemSelection
{
    class FilterSet
    {
        public List<Filter> appliedFilters = new List<Filter>();
    }

    // check what actually makes sense here
    public enum FilterConnection
    {
        AND, OR, XOR, NEG
    }

    public enum FilterOperation
    {
        Include, Exclude,
    }

    class Filter
    {
        public Filter(object[] defsToAdd)
        {
            foreach (var def in defsToAdd)
            {
                if (def is ThingDef)
                {
                    ThingDefs.Add((ThingDef)def);
                    continue;
                }
                else if (def is ThingCategoryDef)
                {
                    ThingCategoryDefs.Add((ThingCategoryDef)def);
                    continue;
                }
            }
        }
        public string Name { get; set; }
        public FilterOperation Operation { get; set; }
        public FilterConnection Connection { get; set; }
        public bool AllowOverride { get; set; }
        public QualityCategory? MaxQuality { get; set; }
        public Action<List<TransferableOneWay>> FilterAction { get; set; }
        public List<ThingDef> ThingDefs = new List<ThingDef>();
        public List<ThingCategoryDef> ThingCategoryDefs = new List<ThingCategoryDef>();
    }
}
