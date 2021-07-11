using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using CaravanAdventures.CaravanItemSelection;
using RimWorld;

namespace CaravanAdventures
{
    static class CompatibilityPatches
    {
        public static List<(string assemblyString, Assembly assembly)> detectedAssemblies;
        public static void ExecuteCompatibilityPatches()
        {
            detectedAssemblies = new List<(string, Assembly)>();

            Helper.RunSavely(() => {
                var alienRaceAssembly = Helper.GetAssembly("alienrace", detectedAssemblies); 
                if (alienRaceAssembly != null && ModSettings.caravanFormingFilterSelectionEnabled)
                {
                    Log.Message($"Caravan Adventures: Applying patch for AlienRaces - adding alienrace corpses to caravan dialog filter");
                    FilterCombs.packUp.appliedFilters.FirstOrDefault(filter => filter.Name == "Corpses").ThingCategoryDefs.Add(ThingCategoryDef.Named("alienCorpseCategory"));
                }
            });

            Helper.RunSavely(() =>
            {
                var simpleSearchBarAssembly = Helper.GetAssembly("simplesearchbar", detectedAssemblies);
                if (simpleSearchBarAssembly != null && ModSettings.caravanFormingFilterSelectionEnabled)
                {
                    Log.Message($"Caravan Adventures: Applying patch for SimpleSearchBar - smaller caravan button layout to make room for search bar");
                    Patches.AutomaticItemSelection.smallLayoutCompatibility = true;
                }
            });

            Helper.RunSavely(() => {
                var alphaBiomesAssembly = Helper.GetAssembly("alphabiomes", detectedAssemblies);
                if (alphaBiomesAssembly != null)
                {
                    Log.Message($"Caravan Adventures: Applying patch to add Alpha Biomes to excluded BiomeDefs for Story Shrine Generation");
                    foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefsListForReading
                        .Where(def => def.modContentPack.assemblies.loadedAssemblies.FirstOrDefault(ass => ass == alphaBiomesAssembly) != null))
                    {
                        CompatibilityDefOf.CACompatDef.excludedBiomeDefNamesForStoryShrineGeneration.Add(biomeDef.defName);
                    }
                }
            });
        }

        public static void ExecuteHarmonyCompatibilityPatches(Harmony harmony)
        {
            
        }
    }
    
}
