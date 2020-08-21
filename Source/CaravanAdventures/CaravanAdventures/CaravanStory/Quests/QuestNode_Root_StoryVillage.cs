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
    class QuestNode_Root_StoryVillage : QuestNode
    {
        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            var slate = QuestGen.slate;

            Log.Message($"Should this be called?");

            if (!StartQuest()) return;
            
            //quest.AddPart();

            //QuestPart_Choice questPart_Choice = quest.RewardChoice(null, null);
            //QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
            //choice.rewards.Add(new Reward_BestowingCeremony
            //{
            //    targetPawnName = pawn.NameShortColored.Resolve(),
            //    titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
            //    awardingFaction = faction,
            //    givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
            //    royalTitle = titleAwardedWhenUpdating
            //});
            //questPart_Choice.choices.Add(choice);
            var pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.FirstOrDefault();
            List<Rule> list3 = new List<Rule>();
            list3.AddRange(GrammarUtility.RulesForPawn("pawn", pawn, null, true, true));
            list3.Add(new Rule_String("newTitle", "test"));
            QuestGen.AddQuestNameRules(list3);
            List<Rule> list4 = new List<Rule>();
            list4.AddRange(GrammarUtility.RulesForFaction("faction", pawn.Faction, true));
            list4.AddRange(GrammarUtility.RulesForPawn("pawn", pawn, null, true, true));
            list4.Add(new Rule_String("newTitle", "test2"));
            list4.Add(new Rule_String("psylinkLevel", "test3"));
            QuestGen.AddQuestDescriptionRules(list4);

            
        }

        private bool StartQuest()
        {
            return Find.TickManager.TicksGame > 1200;
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
