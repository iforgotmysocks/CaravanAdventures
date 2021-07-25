using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.CaravanStory.Quests
{
    [DefOf]
    class StoryQuestDefOf
    {
#pragma warning disable CS0649
        [MayRequireRoyalty]
        public static QuestScriptDef CA_StoryVillage_Arrival;
        [MayRequireRoyalty]
        public static QuestScriptDef CA_TradeCaravan;
        [MayRequireRoyalty]
        public static QuestScriptDef CA_TheTree;
        [MayRequireRoyalty]
        public static QuestScriptDef CA_FindAncientShrine;
    }
}
