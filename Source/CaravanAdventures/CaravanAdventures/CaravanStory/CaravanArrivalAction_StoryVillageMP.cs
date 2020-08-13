using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class CaravanArrivalAction_StoryVillageMP : CaravanArrivalAction
    {
		public override string Label => "VisitAncientMasterShrine".Translate(storyVillageMP.Label);
		public override string ReportString => "CaravanVisiting".Translate(storyVillageMP.Label);
		private StoryVillageMP storyVillageMP;

		public CaravanArrivalAction_StoryVillageMP()
		{
		}

		public CaravanArrivalAction_StoryVillageMP(StoryVillageMP ancientMasterShrine)
		{
			this.storyVillageMP = ancientMasterShrine;
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, StoryVillageMP ancientMasterShrine)
		{
			return ancientMasterShrine != null && ancientMasterShrine.Spawned;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.storyVillageMP != null && storyVillageMP.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(caravan, storyVillageMP);
		}

		public override void Arrived(Caravan caravan)
		{
			this.storyVillageMP.Notify_CaravanArrived(caravan);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref storyVillageMP, "ancientMasterShrine", false);
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, StoryVillageMP ancientMasterShrine)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() 
				=> CanVisit(caravan, ancientMasterShrine), 
				() => new CaravanArrivalAction_StoryVillageMP(ancientMasterShrine), "VisitAncientMasterShrine".Translate(ancientMasterShrine.Label), 
				caravan, ancientMasterShrine.Tile, ancientMasterShrine, null);
		}


	}
}
