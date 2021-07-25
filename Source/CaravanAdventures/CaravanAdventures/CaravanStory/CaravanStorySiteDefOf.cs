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
    class CaravanStorySiteDefOf
    {
#pragma warning disable CS0649
        [MayRequireRoyalty]
        public static WorldObjectDef CAAncientMasterShrineWO;
        [MayRequireRoyalty]
        public static WorldObjectDef CAAncientMasterShrineMP;
        [MayRequireRoyalty]
        public static MapGeneratorDef CAAncientMasterShrineMG;
        [MayRequireRoyalty]
        public static WorldObjectDef CAStoryVillageMP;
        [MayRequireRoyalty]
        public static MapGeneratorDef CAStoryVillageMG;
        [MayRequireRoyalty]
        public static WorldObjectDef CALastJudgmentMP;
        [MayRequireRoyalty]
        public static MapGeneratorDef CALastJudgmentMG;
    }
}
