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
        public readonly float baseDelayFriendlyCaravan = Helper.Debug() ? 1000f : 60000f * 5f;
        public readonly float baseDelayFurtherFriendlyCaravan = Helper.Debug() ? 60000f * 3f : 60000f * 10f;
        public float friendlyCaravanCounter = -1f;
        public Pawn storyContactBondedPawn;

        public void ExposeData()
        {
            Scribe_Values.Look(ref friendlyCaravanCounter, "friendlyCaravanCounter", -1);
            // todo used to be deep saved due to reference causing errors now reverted back to reference - maybe revert to reference when it's clear dafuq is going on
            Scribe_References.Look(ref storyContactBondedPawn, "storyContactBondedPawn");
        }

        public QuestCont_FriendlyCaravan()
        {

        }

        public void AssignCaravanDialog()
        {
            if (CompCache.StoryWC.questCont?.Village?.StoryContact == null)
            {
                Log.Warning("Skipping AssignCaravanDialog, pawn doesn't exist");
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
            var talkSetToAdd = new TalkSet()
            {
                Id = "FriendlyCaravan_PawnDia",
                Addressed = CompCache.StoryWC.questCont.Village.StoryContact,
                Initiator = null,
                ClassName = typeof(QuestCont_FriendlyCaravan).ToString(),
                MethodName = "StoryCharDialog",
                Repeatable = false,
            };
            if (comp.actionsCt.Any(action => action.Id == talkSetToAdd.Id)) Log.Warning($"CompTalk dialog id: {talkSetToAdd.Id} already exists");
            else
            {
                comp.actionsCt.Add(talkSetToAdd);
                comp.ShowQuestionMark = true;
                comp.Enabled = true;
            }
        }

        public void FriendlyCaravan_Conversation(Pawn initiator, Pawn addressed)
        {
            // dialog ideas: 
            // -> currently constructing village nearby
            // -> looking for allies to help explore
            // -> missing her homeworld, lost her husband
            // -> mechs seem to be very aggressive 

            // -> maybe turn next quest into a help call instead of a "visit"

            // todo give main char good equipment

            DiaNode diaNode = null;

            var diaNode6_1 = new DiaNode("TradeCaravan_Dia1_6_1".Translate(addressed.NameShortColored, ThingDefOf.VanometricPowerCell.LabelCap.ToString().Colorize(UnityEngine.Color.green), GenderUtility.GetPossessive(addressed.gender)));
            diaNode6_1.options.Add(new DiaOption("TradeCaravan_Dia1_6_1_Option1".Translate()) { action = () => ConversationFinished(initiator, addressed, true), resolveTree = true });
            var diaNode6_2 = new DiaNode("TradeCaravan_Dia1_6_2".Translate(addressed.NameShortColored));
            diaNode6_2.options.Add(new DiaOption("TradeCaravan_Dia1_6_2_Option1".Translate()) { action = () => ConversationFinished(initiator, addressed), resolveTree = true });

            var diaNode5 = new DiaNode("TradeCaravan_Dia1_5".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode5.options.Add(new DiaOption("TradeCaravan_Dia1_5_Option1".Translate()) { action = () => StoryUtility.AdjustGoodWill(75), link = diaNode6_1 });
            diaNode5.options.Add(new DiaOption("TradeCaravan_Dia1_5_Option2".Translate()) { action = () => StoryUtility.AdjustGoodWill(50), link = diaNode6_2 });

            var diaNode4 = new DiaNode("TradeCaravan_Dia1_4".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode4.options.Add(new DiaOption("TradeCaravan_Dia1_4_Option1".Translate(addressed.NameShortColored, initiator.NameShortColored)) { link = diaNode5 });

            var diaNode3 = new DiaNode("TradeCaravan_Dia1_3".Translate(addressed.NameShortColored));
            diaNode3.options.Add(new DiaOption("TradeCaravan_Dia1_3_Option1".Translate()) { link = diaNode4 });
            diaNode3.options.Add(new DiaOption("TradeCaravan_Dia1_3_Option2".Translate()) { action = () => { ConversationFinished(initiator, addressed); StoryUtility.AdjustGoodWill(30); }, resolveTree = true });

            var diaNode2 = new DiaNode("TradeCaravan_Dia1_2".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode2.options.Add(new DiaOption("TradeCaravan_Dia1_2_Option1".Translate()) { link = diaNode3 });

            diaNode = new DiaNode("TradeCaravan_Dia1_1".Translate(addressed.NameShortColored));
            diaNode.options.Add(new DiaOption("TradeCaravan_Dia1_1_Option1".Translate()) { link = diaNode2 }); ;

            TaggedString taggedString = "TradeCaravan_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void ConversationFinished(Pawn initiator, Pawn addressed, bool reward = false)
        {
            // todo maybe get lord and have them leave early?

            CompCache.StoryWC.questCont.FriendlyCaravan.storyContactBondedPawn = initiator;
            CompCache.StoryWC.SetSF("TradeCaravan_DialogFinished");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedTo".Translate(addressed.NameShortColored));
            if (reward)
            {
                var cell = ThingMaker.MakeThing(ThingDefOf.VanometricPowerCell);
                cell.SetFaction(Faction.OfPlayer);
                cell = cell.TryMakeMinified();
                GenSpawn.Spawn(cell, addressed.Position, addressed.Map, WipeMode.VanishOrMoveAside);
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedToReward".Translate(addressed.NameShortColored, ThingDefOf.VanometricPowerCell.LabelCap.ToString().Colorize(UnityEngine.Color.green)));

            }
            Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_TradeCaravan);
        }

        public void TryCreateFriendlyCaravan(ref float friendlyCaravanCounter, Map map = null)
        {
            if (CompCache.StoryWC.storyFlags["TradeCaravan_DialogFinished"]) return;
             
            DLog.Message($"creating caravan");
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                DLog.Message($"Skipping, Sac hunters are hostile.");
                friendlyCaravanCounter = 20000f;
                return;
            }
            var selectedMap = map ?? Find.Maps.Where(cmap => cmap.ParentFaction == Faction.OfPlayerSilentFail)?.OrderByDescending(cmap => cmap.wealthWatcher.WealthItems)?.FirstOrDefault();
            if (selectedMap == null)
            {
                DLog.Message($"Story caravan couldn't be created, no player map found");
                friendlyCaravanCounter = 1000f;
                return;
            }

            if (!CompCache.StoryWC.storyFlags["TradeCaravan_Arrived"]) QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_TradeCaravan, true, "TradeCaravanQuestName", null, "TradeCaravanQuestDesc");
            StoryUtility.EnsureSacrilegHunters();
            StoryUtility.AssignDialog("FriendlyCaravan_Conversation",
               CompCache.StoryWC.questCont.Village.StoryContact,
               typeof(QuestCont_FriendlyCaravan).ToString(),
               "FriendlyCaravan_Conversation");
            DLog.Message($"added conv to mainpawn {CompCache.StoryWC.questCont.Village.StoryContact.Name}");

            //Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrived".Translate(addressed.NameShortColored));

            var incidentParms = new IncidentParms
            {
                target = selectedMap,
                points = 3000f,
                faction = StoryUtility.FactionOfSacrilegHunters,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
            };

            StoryDefOf.CAFriendlyCaravan.Worker.TryExecute(incidentParms);
            CompCache.StoryWC.SetSF("TradeCaravan_Arrived");
            friendlyCaravanCounter = baseDelayFurtherFriendlyCaravan;
        }

    
    }
}
