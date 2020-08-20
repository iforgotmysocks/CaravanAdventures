using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Grammar;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestNode_Root_StoryVillage : QuestNode
    {
        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            var slate = QuestGen.slate;
            //quest.AddPart();

            QuestPart_Choice questPart_Choice = quest.RewardChoice(null, null);
            QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
            choice.rewards.Add(new Reward_BestowingCeremony
            {
                targetPawnName = pawn.NameShortColored.Resolve(),
                titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
                awardingFaction = faction,
                givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
                royalTitle = titleAwardedWhenUpdating
            });
            questPart_Choice.choices.Add(choice);
            List<Rule> list3 = new List<Rule>();
            list3.AddRange(GrammarUtility.RulesForPawn("pawn", pawn, null, true, true));
            list3.Add(new Rule_String("newTitle", titleAwardedWhenUpdating.GetLabelCapFor(pawn)));
            QuestGen.AddQuestNameRules(list3);
            List<Rule> list4 = new List<Rule>();
            list4.AddRange(GrammarUtility.RulesForFaction("faction", faction, true));
            list4.AddRange(GrammarUtility.RulesForPawn("pawn", pawn, null, true, true));
            list4.Add(new Rule_String("newTitle", pawn.royalty.GetTitleAwardedWhenUpdating(faction, pawn.royalty.GetFavor(faction)).GetLabelFor(pawn)));
            list4.Add(new Rule_String("psylinkLevel", titleAwardedWhenUpdating.maxPsylinkLevel.ToString()));
            QuestGen.AddQuestDescriptionRules(list4);
        }

        protected override bool TestRunInt(Slate slate)
        {
            throw new NotImplementedException();
        }
    }
}
