using RimWorld;
using Verse;

#pragma warning disable CS0649
namespace CaravanAdventures.CaravanStory
{
    [DefOf]
    class StoryDefOf
    {
        [MayRequireRoyalty]
        public static FactionDef CASacrilegHunters;
        [MayRequireRoyalty]
        public static IncidentDef CAFriendlyCaravan;
        [MayRequireRoyalty]
        public static IncidentDef CAUnusualInfestation;

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
        [MayRequireRoyalty]
        public static GameConditionDef CAGameCondition_Apocalypse;

        [MayRequireRoyalty]
        public static ThingDef CAShrinePortal;

        [MayRequireRoyalty]
        public static PawnKindDef CAEndBossMech;

        [MayRequireRoyalty]
        public static PawnGroupKindDef CAMechanoidPawnGroupKindCombatMixed;
        [MayRequireRoyalty]
        public static IncidentDef CAMechRaidMixed;
    }
}