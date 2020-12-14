using CaravanAdventures.CaravanIncidents;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

#pragma warning disable CS0649
namespace CaravanAdventures.CaravanStory
{
    [DefOf]
    class StoryDefOf
    {
        public static FactionDef CASacrilegHunters;
        public static IncidentDef CAFriendlyCaravan;

        public static PawnKindDef CASacrilegHunters_Town_Councilman;
        public static PawnKindDef CASacrilegHunters_Villager;
        public static PawnKindDef CASacrilegHunters_Hunter;
        public static PawnKindDef CASacrilegHunters_HunterVillage;
        public static PawnKindDef CASacrilegHunters_ExperiencedHunter;
        public static PawnKindDef CASacrilegHunters_ExperiencedHunterVillage;

        public static GameConditionDef CAGameCondition_Apocalypse;

        public static ThingDef CAShrinePortal;

        public static PawnKindDef CAEndBossMech;

        public static PawnGroupKindDef CAMechanoidPawnGroupKindCombatMixed;

        public static IncidentDef CAMechRaidMixed;
    }
}