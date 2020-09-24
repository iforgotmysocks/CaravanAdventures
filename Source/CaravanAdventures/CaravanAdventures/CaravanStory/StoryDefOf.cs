using CaravanAdventures.CaravanIncidents;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    [DefOf]
    class StoryDefOf
    {
#pragma warning disable CS0649
        public static FactionDef SacrilegHunters;
        public static IncidentDef CAFriendlyCaravan;

        public static PawnKindDef SacrilegHunters_Town_Councilman;
        public static PawnKindDef SacrilegHunters_Villager;
        public static PawnKindDef SacrilegHunters_ExperiencedHunter;
        public static PawnKindDef SacrilegHunters_ExperiencedHunterVillage;
    }
}