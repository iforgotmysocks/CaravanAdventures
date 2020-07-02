using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaravanAdventures.CaravanItemSelection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using CaravanAdventures.Patches;

namespace CaravanAdventures.CaravanItemSelection
{
    class FilterCombs
    {
        public static FilterSet packUp = new FilterSet();

        public static void InitFilterSets()
        {
            packUp.appliedFilters.Add(new Filter(new object[] { ThingCategoryDefOf.Chunks, ThingCategoryDefOf.StoneChunks })
            {
                Name = "Chunks",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] { ThingCategoryDefOf.Corpses, ThingCategoryDefOf.CorpsesAnimal, ThingCategoryDefOf.CorpsesHumanlike, ThingCategoryDefOf.CorpsesInsect, ThingCategoryDefOf.CorpsesMechanoid })
            {
                Name = "Corpses",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] { ThingCategoryDefOf.ResourcesRaw, ThingCategoryDefOf.StoneBlocks, ThingCategoryDefOf.Buildings })
            {
                Name = "BuildMaterials",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Exclude,
            });
            packUp.appliedFilters.Add(new Filter(new object[] { ThingDefOf.Gold, ThingDefOf.Silver, ThingDefOf.Plasteel})
            {
                Name = "IncludeBuildMaterials",
                Connection = FilterConnection.OR,
                Operation = FilterOperation.Include,
            });
        }

        public static void ApplyAll(List<Patches.Section> sections)
        {
            foreach (var section in sections)
                foreach (var trans in section.transferables)
                    FilterHelper.SetMaxAmount(trans);
        }

        public static void ApplyPackUp(List<Patches.Section> sections)
        {
            foreach (var section in sections)
            {
                foreach (var trans in section.transferables)
                {
                    if (FilterHelper.DoFiltersApply(packUp, trans)) FilterHelper.SetMaxAmount(trans);
                    else FilterHelper.SetMinAmount(trans);
                }
            }
        }

        public static void ApplyNone(List<Patches.Section> sections)
        {
            foreach (var section in sections)
                foreach (var trans in section.transferables)
                    FilterHelper.SetMinAmount(trans);
        }
    }
}
