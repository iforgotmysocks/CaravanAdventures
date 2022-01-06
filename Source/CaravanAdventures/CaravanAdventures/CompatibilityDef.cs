using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CaravanAdventures
{
#pragma warning disable CS0649
    class MechanoidBounty
    {
        public string raceDefName;
        public float bountyPoints;
    }

    class CompatibilityDef : Def
    {
        public List<string> additionalBountyFactionDefsToAdd = new List<string>();
        public List<MechanoidBounty> mechanoidBountyToAdd = new List<MechanoidBounty>();
        public List<string> raceDefsToExcludeFromTravelCompanions = new List<string>();
        public List<string> racesWithModExtsToExcludeFromTravelCompanions = new List<string>();
        public List<string> excludedBiomeDefNamesForStoryShrineGeneration = new List<string>();
        public List<string> raceDefsToExcludeFromAncientCoordinator = new List<string>();
        public List<string> racesWithModExtsToExcludeFromAncientCoordinator = new List<string>();
    }

    [DefOf]
    class CompatibilityDefOf
    {
        public static CompatibilityDef CACompatDef;
    }
}
