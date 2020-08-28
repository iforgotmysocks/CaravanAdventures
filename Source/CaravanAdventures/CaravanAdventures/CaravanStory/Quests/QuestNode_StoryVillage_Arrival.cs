using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestNode_StoryVillage_Arrival : QuestNode
    {
        public const string questTag = "StoryVillage";

        protected override void RunInt()
        {
            //var quest = QuestGen.quest;
            //var slate = QuestGen.slate;
            if (!StartQuest()) return;

            var questGirl = StoryWC.StoryContact;
            List<Rule> rules = new List<Rule>();
            rules.Add(new Rule_String("faction_name", questGirl.Faction.Name.ToString()));
            rules.AddRange(GrammarUtility.RulesForPawn("pawn", questGirl, null, true, true));
            QuestGen.AddQuestDescriptionRules(rules);
        }

        private bool StartQuest()
        {
            return !StoryWC.storyFlags["IntroVillage_Created"];
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

    }
}
