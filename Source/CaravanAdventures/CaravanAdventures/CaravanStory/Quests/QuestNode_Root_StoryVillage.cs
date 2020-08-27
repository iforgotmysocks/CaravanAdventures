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
        public const string questTag = "StoryVillage";

        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            var slate = QuestGen.slate;

            Log.Message($"Should this be called?");

            if (!StartQuest()) return;

            var questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("StoryVillage");
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
            var questGirl = GenerateQuestGirl();
            RimWorld.QuestUtility.AddQuestTag(ref questGirl.questTags, questTag);
            slate.Set<Pawn>("questGirl", questGirl);
            //List<Rule> list3 = new List<Rule>();
            //list3.AddRange(GrammarUtility.RulesForPawn("pawn", questGirl, null, true, true));
            //QuestGen.AddQuestNameRules(list3);
            List<Rule> list4 = new List<Rule>();
            //list4.AddRange(GrammarUtility.RulesForFaction("faction", questGirl.Faction, true));
            list4.Add(new Rule_String("faction_name", questGirl.Faction.Name.ToString()));
            list4.AddRange(GrammarUtility.RulesForPawn("pawn", questGirl, null, true, true));
            QuestGen.AddQuestDescriptionRules(list4);

            var questPart_StoryVillage_Arrived = new QuestPart_StoryVillage_Arrived() { Girl = questGirl, QuestTag = questTag };
            questPart_StoryVillage_Arrived.inSignal_Arrived = QuestGenUtility.HardcodedSignalWithQuestID("village.Arrived");
            quest.AddPart(questPart_StoryVillage_Arrived);

        }

        // todo move to storyutil; just create a new quest for each step, fuck this
        private Pawn GenerateQuestGirl()
        {
            var girl = PawnGenerator.GeneratePawn(new PawnGenerationRequest()
            {
                FixedBiologicalAge = 22,
                FixedChronologicalAge = 3022,
                FixedGender = Gender.Female,
                AllowAddictions = false,
                AllowGay = false,
                AllowDead = false,
                Faction = StoryUtility.EnsureSacrilegHunters(FactionRelationKind.Ally),
                KindDef = PawnKindDefOf.Villager,
            });

            // todo looks?
            //girl.story.hairDef = 

            girl.story.traits.allTraits.RemoveAll(x => x.def == TraitDefOf.Beauty);
            girl.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));

            return girl;
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
