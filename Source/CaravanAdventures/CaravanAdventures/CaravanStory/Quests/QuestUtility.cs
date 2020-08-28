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
            //slate.Set<Pawn>("titleHolder", pawn, false);
            //slate.Set<Faction>("bestowingFaction", faction, false);
            if (DefDatabase<QuestScriptDef>.GetNamed("CA_StoryVillage_Arrival").CanRun(slate))
            {
                RimWorld.QuestUtility.SendLetterQuestAvailable(RimWorld.QuestUtility.GenerateQuestAndMakeAvailable(DefDatabase<QuestScriptDef>.GetNamed("CA_StoryVillage_Arrival"), slate));
            }
        }

        public static void GenerateStoryQuest(QuestScriptDef questDef)
        {
            Slate slate = new Slate();
            if (questDef.CanRun(slate))
            {
                RimWorld.QuestUtility.SendLetterQuestAvailable(RimWorld.QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate));
            }
        }

        public static void CompleteQuest(QuestScriptDef questDef, QuestEndOutcome outcome = QuestEndOutcome.Success, QuestState state = QuestState.Ongoing, bool ignoreStateCompleteAnyway = false,  bool sendLetter = true)
        {
            Log.Message($"Signalling to end quest");
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef
            && (x.State == state || ignoreStateCompleteAnyway));
            if (quest != null) quest.End(outcome, sendLetter);
        }

        public static void AppendQuestDescription(QuestScriptDef questDef, TaggedString text)
        {
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef);
            if (quest != null) quest.description += text;
        }
    }
}
