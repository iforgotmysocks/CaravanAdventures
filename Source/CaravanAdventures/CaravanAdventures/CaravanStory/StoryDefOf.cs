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
        public static FactionDef CASacrilegHunters;
        public static IncidentDef CAFriendlyCaravan;

        public static PawnKindDef CASacrilegHunters_Town_Councilman;
        public static PawnKindDef CASacrilegHunters_Villager;
        public static PawnKindDef CASacrilegHunters_ExperiencedHunter;
        public static PawnKindDef CASacrilegHunters_ExperiencedHunterVillage;

        public static GameConditionDef CAGameCondition_Apocalypse;

        public static ThingDef CAShrinePortal;

        public static PawnKindDef CAEndBossMech;
    }
}