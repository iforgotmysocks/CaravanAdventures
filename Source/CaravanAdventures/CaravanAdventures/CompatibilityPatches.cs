using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using CaravanAdventures.CaravanItemSelection;
using RimWorld;

namespace CaravanAdventures
{
    static class CompatibilityPatches
    {
        public static List<(string assemblyString, Assembly assembly)> detectedAssemblies;
        public static bool InDetectedAssemblies(string assName, bool caseSensitive = false) => caseSensitive ? detectedAssemblies.Any(x => x.assemblyString == assName) : detectedAssemblies.Any(x => x.assemblyString.ToLower() == assName.ToLower());
        public static void ExecuteCompatibilityPatches()
        {
            detectedAssemblies = detectedAssemblies ?? new List<(string, Assembly)>();

            Helper.RunSavely(() =>
            {
                var alienRaceAssembly = Helper.GetAssembly("alienrace", detectedAssemblies);
                if (alienRaceAssembly != null && ModSettings.caravanFormingFilterSelectionEnabled)
                {
                    Log.Message($"Caravan Adventures: Applying patch for AlienRaces - adding alienrace corpses to caravan dialog filter");
                    FilterCombs.packUp.appliedFilters.FirstOrDefault(filter => filter.Name == "Corpses").ThingCategoryDefs.Add(ThingCategoryDef.Named("alienCorpseCategory"));
                }
            }, false, ErrorMessage("alien races"));

            Helper.RunSavely(() =>
            {
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
            }, false, ErrorMessage("alpha biomes"));

            Helper.RunSavely(() =>
            {
                var rgwWastelandAssembly = Helper.GetAssembly("RGW_Wasteland", detectedAssemblies);
                if (rgwWastelandAssembly != null)
                {
                    Log.Message($"Caravan Adventures: Applying patch to add Regrowth to excluded BiomeDefs for Story Shrine Generation");
                    foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefsListForReading
                        .Where(def => def.modContentPack.assemblies.loadedAssemblies.FirstOrDefault(ass => ass == rgwWastelandAssembly) != null))
                    {
                        CompatibilityDefOf.CACompatDef.excludedBiomeDefNamesForStoryShrineGeneration.Add(biomeDef.defName);
                    }
                }
            }, false, ErrorMessage("Regrowth - Wasteland"));

            Helper.RunSavely(() =>
            {
                var realRuinsAssembly = Helper.GetAssembly("realruins", detectedAssemblies);
                if (realRuinsAssembly != null)
                {
                    if (CaravanStory.CaravanStorySiteDefOf.CAAncientMasterShrineMG.genSteps.RemoveAll(x => x.defName == "ScatterRealRuins") == 1)
                    {
                        Log.Message($"Caravan Adventures: Applying patch to remove realruins from CA story shrine maps");
                    }
                    else Log.Warning($"Caravan Adventures: Applying patch for realruins failed");
                }
            }, false, "Following error happend while trying to patching SOS2 for compatibility with CA, but was catched savely:");

            Helper.RunSavely(() =>
            {
                var sos2Assembly = Helper.GetAssembly("shipshaveinsides", detectedAssemblies);
                if (sos2Assembly != null && ModSettings.storyEnabled)
                {
                    Log.Message($"Adding SOS2 story check for space");
                }
            }, false, ErrorMessage("SOS2"));

            ExecuteHarmonyCompatibilityPatches();

            Log.Message($"CA patches complete. v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd(new[] { '.', '0' })} (1.2)");
        }

        public static void ExecuteHarmonyCompatibilityPatches()
        {
            detectedAssemblies = detectedAssemblies ?? new List<(string, Assembly)>();

            // todo - remove friendly faction detection patch after faction removal
            Helper.RunSavely(() =>
            {
                var vfeCoreAssembly = Helper.GetAssembly("VFECore", detectedAssemblies);
                if (vfeCoreAssembly != null && ModSettings.storyEnabled)
                {
                    Log.Message($"Patching VFE from bringing up the faction dialog for no longer needed faction");
                    Patches.Compatibility.VFECoreFriendlyFactionDetectionPatch.ApplyPatches(vfeCoreAssembly);
                }
            }, false, ErrorMessage("VFECore"));

            Helper.RunSavely(() =>
            {
                var vsewwAssembly = Helper.GetAssembly("VSEWW", detectedAssemblies);
                if (vsewwAssembly != null && ModSettings.storyEnabled)
                {
                    Log.Message($"Fixing Winston Waves prefix NRE for story and bounty raids");
                    Patches.Compatibility.WinstonWavesPatch.ApplyPatches(vsewwAssembly);
                }
            }, false, ErrorMessage("Winston Waves"));

            Helper.RunSavely(() =>
            {
                if (ModSettings.storyEnabled && detectedAssemblies.Any(x => x.assemblyString == "shipshaveinsides"))
                {
                    Log.Message($"Applying CA Ancient Ability adjustments for space");
                    Patches.Compatibility.SoS2Patch.ApplyPatches(detectedAssemblies.FirstOrDefault(x => x.assemblyString == "shipshaveinsides").assembly);
                }
            }, false, ErrorMessage("SoS2"));

            Helper.RunSavely(() =>
            {
                var rimWarAssembly = Helper.GetAssembly("RimWar", detectedAssemblies);
                if (ModSettings.storyEnabled && rimWarAssembly != null)
                {
                    Log.Message($"Enabling rimwar patch to avoid sacrileg hunters being chosen as the victory faction at gamestart");
                    Patches.Compatibility.RimWarPatch.ApplyPatches(rimWarAssembly);
                }
            }, false, ErrorMessage("RimWar"));
        }

        private static string ErrorMessage(string modName) => $"Following error happend while trying to patch {modName} for compatibility with CA, but was catched savely:\n";

    }

}
