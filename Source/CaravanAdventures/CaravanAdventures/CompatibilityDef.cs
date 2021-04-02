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
        public List<MechanoidBounty> mechanoidBountyToAdd = new List<MechanoidBounty>();
        public List<ThingDef> raceDefsToExcludeFromTravelCompanions = new List<ThingDef>();
        public List<string> racesWithModExtsToExcludeFromTravelCompanions = new List<string>();
    }

    [DefOf]
    class CompatibilityDefOf
    {
        public static CompatibilityDef CACompatDef;
    }
}
