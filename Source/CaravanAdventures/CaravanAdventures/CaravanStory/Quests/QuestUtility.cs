using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestUtility
    {
        public static void GenerateStoryQuest(QuestScriptDef questDef, bool directlyAccept = true, string questName = "", object[] questNameParms = null, string questDescription = "", object[] questDescriptionParms = null, bool ignoreOneQuestPerTypeLimit = false)
        {
            if (Find.QuestManager.QuestsListForReading.Any(quest => quest.root == questDef) && !ignoreOneQuestPerTypeLimit)
            {
                DLog.Warning($"Tried to start Quest {questDef.defName} despite it already exists. Skipping...");
                return;
            }
            Slate slate = new Slate();
            if (questDef.CanRun(slate))
            {
                var quest = RimWorld.QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);
                if (!string.IsNullOrEmpty(questName)) quest.name = questName.Translate(GetNamedArgumentsFromObjects(questNameParms));
                if (!string.IsNullOrEmpty(questDescription)) quest.description = questDescription.Translate(GetNamedArgumentsFromObjects(questDescriptionParms));
                RimWorld.QuestUtility.SendLetterQuestAvailable(quest);
                if (directlyAccept) quest.Accept(null);
            }
        }

        private static NamedArgument[] GetNamedArgumentsFromObjects(object[] questNameParms)
        {
            var args = new NamedArgument[questNameParms == null ? 0 : questNameParms.Length];
            if (questNameParms != null)
            {
                for (int i = 0; i < questNameParms.Length; i++)
                {
                    args[i] = new NamedArgument(questNameParms[i], questNameParms[i].ToString());
                }
            }
            return args;
        }

        public static void GenerateStoryQuest_old(QuestScriptDef questDef, bool directlyAccept = true)
        {
            Slate slate = new Slate();
            if (questDef.CanRun(slate))
            {
                var quest = RimWorld.QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);
                RimWorld.QuestUtility.SendLetterQuestAvailable(quest);
                if (directlyAccept) quest.Accept(null);
            }
        }

        public static void CompleteQuest(QuestScriptDef questDef, bool addQuestUpdatedNoticeToLetter = true, QuestEndOutcome outcome = QuestEndOutcome.Success, QuestState state = QuestState.Ongoing, bool ignoreStateCompleteAnyway = false,  bool sendLetter = true)
        {
            DLog.Message($"Signalling to end quest");
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef
            && (x.State == state || ignoreStateCompleteAnyway));
            if (quest != null)
            {
                if (addQuestUpdatedNoticeToLetter && sendLetter)
                {
                    quest.End(outcome, false);
                    SendOutcomeLetterWithQuestUpdatedNotice(quest);
                }
                else quest.End(outcome, sendLetter);
            }
        }

        private static void SendOutcomeLetterWithQuestUpdatedNotice(Quest quest)
        {
            string key = null;
            string key2 = null;
            LetterDef textLetterDef = null;
            switch (quest.State)
            {
                case QuestState.EndedUnknownOutcome:
                    key2 = "LetterQuestConcludedLabelWithUpdateInfo";
                    key = "LetterQuestCompletedConcluded";
                    textLetterDef = LetterDefOf.NeutralEvent;
                    SoundDefOf.Quest_Concluded.PlayOneShotOnCamera(null);
                    break;
                case QuestState.EndedSuccess:
                    key2 = "LetterQuestCompletedLabelWithUpdateInfo";
                    key = "LetterQuestCompletedSuccess";
                    textLetterDef = LetterDefOf.PositiveEvent;
                    SoundDefOf.Quest_Succeded.PlayOneShotOnCamera(null);
                    break;
                case QuestState.EndedFailed:
                    key2 = "LetterQuestFailedLabelWithUpdateInfo";
                    key = "LetterQuestCompletedFail";
                    textLetterDef = LetterDefOf.NegativeEvent;
                    SoundDefOf.Quest_Failed.PlayOneShotOnCamera(null);
                    break;
            }
            Find.LetterStack.ReceiveLetter(key2.Translate(), key.Translate(quest.name.CapitalizeFirst()), textLetterDef, null, null, quest, null, null);
        }

        public static void DeleteQuest(QuestScriptDef questDef, QuestEndOutcome outcome = QuestEndOutcome.Success, QuestState state = QuestState.Ongoing, bool ignoreStateCompleteAnyway = true, bool sendLetter = true)
        {
            DLog.Message($"Deleting quest");
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef
            && (x.State == state || ignoreStateCompleteAnyway));
            if (quest != null) Find.QuestManager.Remove(quest);
        }

        public static void AppendQuestDescription(QuestScriptDef questDef, TaggedString text, bool avoidAdditionalSpaceFormatting = false, bool sendUpdateInfoLetter = false)
        {
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef);
            if (!avoidAdditionalSpaceFormatting) text = "\n\n" + text;
            if (quest != null) quest.description += text;
            if (sendUpdateInfoLetter) Find.LetterStack.ReceiveLetter("CALetterQuestOngoingUpdatedInfo".Translate(), "CALetterQuestOngoingUpdatedInfoText".Translate(quest.name.CapitalizeFirst()), LetterDefOf.NeutralEvent, null, null, quest, null, null);
        }

        public static void UpdateQuestLocation(QuestScriptDef questDef, WorldObject location, bool clearExistingLocations = true)
        {
            var quest = Find.QuestManager.QuestsListForReading.FirstOrDefault(x => x.root == questDef);
            if (quest == null) return;

            var locationPart = quest.PartsListForReading.OfType<QuestPart_AddLocations>().FirstOrDefault();
            if (locationPart == null) {
                //quest.RemovePart(locationLink);
                locationPart = new QuestPart_AddLocations();
                quest.AddPart(locationPart);
            }
            if (clearExistingLocations) locationPart.Locations.Clear();
            locationPart.Locations.Add(location);
        }
    }
}
