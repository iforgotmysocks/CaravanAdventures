using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestUtility
    {
        public static void GenerateStoryVillageQuest(Pawn pawn, Faction faction)
        {
            Slate slate = new Slate();
            slate.Set<Pawn>("titleHolder", pawn, false);
            slate.Set<Faction>("bestowingFaction", faction, false);
            if (DefDatabase<QuestScriptDef>.GetNamed("CAStoryVillage").CanRun(slate))
            {
                RimWorld.QuestUtility.SendLetterQuestAvailable(RimWorld.QuestUtility.GenerateQuestAndMakeAvailable(DefDatabase<QuestScriptDef>.GetNamed("CAStoryVillage"), slate));
            }
        }
    }
}
