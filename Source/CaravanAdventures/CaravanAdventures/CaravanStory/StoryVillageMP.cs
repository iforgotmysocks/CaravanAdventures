using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory
{
	class StoryVillageMP : Settlement
	{

		private bool temporaryDeleteTalkedToContactPawn = true;
		private int ticks;

		public StoryVillageMP()
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref ticks, "ticks");
        }

        public void Notify_CaravanArrived(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate ()
			{
				Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, null);
				var label = "StoryVillageArrivedLetterTitle".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID()));
				var text = "StoryVillageArrivedLetterMessage".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())).CapitalizeFirst();
			
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, Faction, null, null, null);
				CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
				Log.Message($"Got here, must be spawn cell related");
				var storyContactCell = CellFinder.RandomNotEdgeCell(Math.Min(orGenerateMap.Size.x / 2 - (orGenerateMap.Size.x / 6), orGenerateMap.Size.y / 2 - (orGenerateMap.Size.y / 6)), Map);
				
				GenSpawn.Spawn(QuestCont.Village.StoryContact, storyContactCell, orGenerateMap);

				Log.Message($"Got here before crash");
				orGenerateMap.mapPawns.AllPawnsSpawned.Where(x => x.Faction == StoryUtility.EnsureSacrilegHunters() && x.GetLord().LordJob.GetType() == typeof(LordJob_DefendBase)).FirstOrDefault().GetLord().AddPawn(QuestCont.Village.StoryContact);

				StoryWC.SetSF("IntroVillage_Entered");
				// todo generate settlement
				// todo generate girl -> or maybe even do that when creating the quest?
				
				// todo test quest stuff
				


				//Map map = StoryUtility.GenerateFriendlyVillage(caravan, Find.World.info.initialMapSize, CaravanStorySiteDefOf.AncientMasterShrineMP);
				//mp = map.Parent as AncientMasterShrineMP;



				// CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true);
				//Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;

				//mp.Init();
			}, "StoryVillageEnterMapMessage".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())), false, null, true);

			// GeneratingMapForNewEncounter
		}

		public override void PostMapGenerate()
        {
           
        }

		public void ConversationFinished(Pawn initiator, Pawn addressed)
		{
			Log.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
			DiaNode diaNode = null;
			var endDiaNodeAccepted = new DiaNode("StoryVillage_Dia1_3".Translate());
			endDiaNodeAccepted.options.Add(new DiaOption("StoryVillage_Dia1_3_Option1".Translate()) { action = () => SpawnMechArmy(initiator, addressed), resolveTree = true });

			var subDiaNode = new DiaNode("StoryVillage_Dia1_2".Translate());
			subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option1".Translate()) { link = endDiaNodeAccepted });
			subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option2".Translate()) { link = endDiaNodeAccepted });

			diaNode = new DiaNode("StoryVillage_Dia1_1".Translate(initiator.NameShortColored));
			diaNode.options.Add(new DiaOption("StoryVillage_Dia1_1_Option1".Translate()) { link = subDiaNode }); ;
			
			TaggedString taggedString = "StoryVillage_Dia1_DiaTitle".Translate(addressed.NameShortColored);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
		}

        private void SpawnMechArmy(Pawn initiator, Pawn addressed)
        {
            
        }

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.StoryVillageMG;

		public override void Tick()
		{
			// todo remove true
			if (base.HasMap)
			{
				if (ticks >= 1200)
				{
					if (temporaryDeleteTalkedToContactPawn)
					{
						//Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "\n\n alskdjflaksf");
						//Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);
					}
					ticks = 0;
				}

				ticks++;
			}
		}

        protected void CreateMap(Caravan caravan)
		{
			
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (var baseOptions in base.GetFloatMenuOptions(caravan))
			{
				yield return baseOptions;
			}

            foreach (var storyVillageOption in CaravanArrivalAction_StoryVillageMP.GetFloatMenuOptions(caravan, this))
            {
                yield return storyVillageOption;
            }
            yield break;
		}

    }
}
