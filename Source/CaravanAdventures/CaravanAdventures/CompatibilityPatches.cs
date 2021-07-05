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

namespace CaravanAdventures
{
    static class CompatibilityPatches
    {
        public static void ExecuteCompatibilityPatches()
        {

            Helper.RunSavely(() => {
                var alienRaceAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("alienrace"));
                if (alienRaceAssembly != null && ModSettings.caravanFormingFilterSelectionEnabled)
                {
                    Log.Message($"Caravan Adventures: Applying patch for AlienRaces - adding alienrace corpses to caravan dialog filter");
                    FilterCombs.packUp.appliedFilters.FirstOrDefault(filter => filter.Name == "Corpses").ThingCategoryDefs.Add(ThingCategoryDef.Named("alienCorpseCategory"));
                }
            });

            //Helper.RunSavely(() => {
            //    var simpleSearchBarAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("simplesearchbar"));
            //    if (simpleSearchBarAssembly != null && ModSettings.caravanFormingFilterSelectionEnabled)
            //    {
            //        Log.Message($"Caravan Adventures: Applying patch for SimpleSearchBar - smaller caravan button layout to make room for search bar");
            //        Patches.AutomaticItemSelection.smallLayoutCompatibility = true;
            //    }
            //});
        }

        public static void ExecuteHarmonyCompatibilityPatches(Harmony harmony)
        {
              
        }
    }
    
}
