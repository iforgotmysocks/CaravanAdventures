using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_FriendlyCaravan : IExposable
    {
        public QuestCont_FriendlyCaravan()
        {

        }

        public void AssignCaravanDialog()
        {
            if (CompCache.StoryWC.questCont?.Village?.StoryContact == null)
            {
                Log.Message("Skipping, pawn doesn't exist");
                return;
            }
            var comp = CompCache.StoryWC.questCont.Village.StoryContact.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on pawn {CompCache.StoryWC.questCont?.Village?.StoryContact?.Name} is null, which shouldn't happen");
                comp = new CompTalk();
                comp.parent = CompCache.StoryWC.questCont.Village.StoryContact;
                CompCache.StoryWC.questCont.Village.StoryContact.AllComps.Add(comp);
            }
            comp.actionsCt.Add(new TalkSet()
            {
                Id = "FriendlyCaravan_PawnDia",
                Addressed = CompCache.StoryWC.questCont.Village.StoryContact,
                Initiator = null,
                ClassName = typeof(QuestCont_FriendlyCaravan).ToString(),
                MethodName = "StoryCharDialog",
                Repeatable = false,
            });
            comp.ShowQuestionMark = true;
            comp.Enabled = true;
        }

        public void StoryCharDialog(Pawn initiator, Pawn addressed)
        {
            // dialog ideas: 
            // -> currently constructing village nearby
            // -> looking for allies to help explore
            // -> missing her homeworld, lost her husband
            // -> mechs seem to be very aggressive 

            // -> maybe turn next quest into a help call instead of a "visit"

            DiaNode diaNode = null;
            var endDiaNodeAccepted = new DiaNode("StoryVillage_Dia1_3".Translate());
            endDiaNodeAccepted.options.Add(new DiaOption("StoryVillage_Dia1_3_Option1".Translate()) { action = () => ConversationFinished(initiator, addressed), resolveTree = true });

            var subDiaNode = new DiaNode("StoryVillage_Dia1_2".Translate());
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option1".Translate()) { link = endDiaNodeAccepted });
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option2".Translate()) { link = endDiaNodeAccepted });

            diaNode = new DiaNode("StoryVillage_Dia1_1".Translate(initiator.NameShortColored));
            diaNode.options.Add(new DiaOption("StoryVillage_Dia1_1_Option1".Translate()) { link = subDiaNode }); ;

            TaggedString taggedString = "StoryVillage_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void ConversationFinished(Pawn initiator, Pawn addressed)
        {
            // todo maybe get lord and have them leave early?
            CompCache.StoryWC.SetSF("TradeCaravan_DialogFinished");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedTo".Translate(addressed.NameShortColored));
            Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_TradeCaravan);
        }

        public void TryCreateFriendlyCaravan(ref float friendlyCaravanCounter, Map map = null)
        {
            QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_TradeCaravan, true, "TradeCaravanQuestName", null, "TradeCaravanQuestDesc");
            
            Log.Message($"creating caravan");
            var selectedMap = map ?? Find.Maps.Where(cmap => cmap.ParentFaction == Faction.OfPlayerSilentFail)?.OrderByDescending(cmap => cmap.wealthWatcher.WealthItems)?.FirstOrDefault();
            if (selectedMap == null)
            {
                Log.Message($"Story caravan couldn't be created, no player map found");
                friendlyCaravanCounter = 1000;
                return;
            }

            StoryUtility.EnsureSacrilegHunters(FactionRelationKind.Ally);

            //Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrived".Translate(addressed.NameShortColored));

            var incidentParms = new IncidentParms
            {
                target = selectedMap,
                points = 14000,
                faction = StoryUtility.FactionOfSacrilegHunters,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
            };
            
            StoryDefOf.CAFriendlyCaravan.Worker.TryExecute(incidentParms);
            CompCache.StoryWC.SetSF("TradeCaravan_Arrived");
        }

        public void ExposeData()
        {

        }
    }
}
