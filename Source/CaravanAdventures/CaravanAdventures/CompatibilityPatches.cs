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
                if (alienRaceAssembly != null)
                {
                    Log.Message($"Caravan Adventures: Applying patch for AlienRaces - adding alienrace corpses to caravan dialog filter");
                    FilterCombs.packUp.appliedFilters.FirstOrDefault(filter => filter.Name == "Corpses").ThingCategoryDefs.Add(ThingCategoryDef.Named("alienCorpseCategory"));
                }
            });
        }

        public static void ExecuteHarmonyCompatibilityPatches(Harmony harmony)
        {
              
        }
    }
    
}
