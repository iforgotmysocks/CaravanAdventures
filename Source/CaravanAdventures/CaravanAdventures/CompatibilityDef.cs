using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }

    [DefOf]
    class CompatibilityDefOf
    {
        public static CompatibilityDef CACompatDef;
    }
}
