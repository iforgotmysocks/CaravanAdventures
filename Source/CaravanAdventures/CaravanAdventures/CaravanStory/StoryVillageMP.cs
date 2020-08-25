using RimWorld;
using RimWorld.Planet;
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
		public StoryVillageMP()
        {

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


				// todo generate settlement
				// todo generate girl -> or maybe even do that when creating the quest?

				// todo test quest stuff
				Find.SignalManager.SendSignal(new Signal("village.Arrived"));


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

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.StoryVillageMG;

        public override void Tick()
        {

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
