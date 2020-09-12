using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestNode_Temp : QuestNode
    {
        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            //var slate = QuestGen.slate;
            if (!StartQuest()) return;

            //var rules = new List<Rule>();
            //Log.Message($"Storycontact name: {CompCache.StoryWC.questCont.Village.StoryContact.Name}");
            //rules.Add(new Rule_String("faction_name", CompCache.StoryWC.questCont.Village.StoryContact.Faction.Name.ToString()));
            //rules.Add(new Rule_String("settlement_name", CompCache.StoryWC.questCont.Village.Settlement.Name.Colorize(UnityEngine.Color.cyan)));
            //rules.AddRange(GrammarUtility.RulesForPawn("pawn", CompCache.StoryWC.questCont.Village.StoryContact, null, true, true));
            //QuestGen.AddQuestDescriptionRules(rules);
        }

        private bool StartQuest()
        {
            return true;
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
