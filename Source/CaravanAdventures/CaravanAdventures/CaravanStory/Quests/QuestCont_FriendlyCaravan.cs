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
            Log.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            var endDiaNodeAccepted = new DiaNode("StoryVillage_Dia1_3".Translate());
            endDiaNodeAccepted.options.Add(new DiaOption("StoryVillage_Dia1_3_Option1".Translate()) { action = () => Log.Message("todo"), resolveTree = true });

            var subDiaNode = new DiaNode("StoryVillage_Dia1_2".Translate());
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option1".Translate()) { link = endDiaNodeAccepted });
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option2".Translate()) { link = endDiaNodeAccepted });

            diaNode = new DiaNode("StoryVillage_Dia1_1".Translate(initiator.NameShortColored));
            diaNode.options.Add(new DiaOption("StoryVillage_Dia1_1_Option1".Translate()) { link = subDiaNode }); ;

            TaggedString taggedString = "StoryVillage_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        public void CreateFriendlyCaravan(Map map)
        {
            CompCache.StoryWC.SetSF("IntroVillage_TalkedToFriend");
            //Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrived".Translate(addressed.NameShortColored));

            var incidentParms = new IncidentParms
            {
                target = map,
                //incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
                points = 32000,
                faction = Faction.OfMechanoids,
                // todo - find out how to ensure mixed pawngroupmakerkinds
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
            };
            Log.Message($"Default threat points: {StorytellerUtility.DefaultThreatPointsNow(incidentParms.target)}");
            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);

            CompCache.StoryWC.SetSF("IntroVillage_MechsArrived");
        }

        public void ExposeData()
        {

        }
    }
}
