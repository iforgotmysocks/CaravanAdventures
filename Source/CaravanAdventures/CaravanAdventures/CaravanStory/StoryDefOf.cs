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
        [MayRequireRoyalty]
        public static FactionDef CASacrilegHunters;
        public static IncidentDef CAFriendlyCaravan;

        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_Town_Councilman;
        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_Villager;
        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_Hunter;
        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_HunterVillage;
        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_ExperiencedHunter;
        [MayRequireRoyalty]
        public static PawnKindDef CASacrilegHunters_ExperiencedHunterVillage;

        public static GameConditionDef CAGameCondition_Apocalypse;

        public static ThingDef CAShrinePortal;

        [MayRequireRoyalty]
        public static PawnKindDef CAEndBossMech;

        [MayRequireRoyalty]
        public static PawnGroupKindDef CAMechanoidPawnGroupKindCombatMixed;

        public static IncidentDef CAMechRaidMixed;
    }
}