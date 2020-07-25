using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class CaravanArrivalAction_AncientMasterShrineWO : CaravanArrivalAction
    {
		public override string Label => "VisitAncientMasterShrine".Translate(ancientMasterShrine.Label);
		public override string ReportString => "CaravanVisiting".Translate(ancientMasterShrine.Label);
		private AncientMasterShrineWO ancientMasterShrine;

		public CaravanArrivalAction_AncientMasterShrineWO()
		{
		}

		public CaravanArrivalAction_AncientMasterShrineWO(AncientMasterShrineWO ancientMasterShrine)
		{
			this.ancientMasterShrine = ancientMasterShrine;
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, AncientMasterShrineWO ancientMasterShrine)
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
			if (this.ancientMasterShrine != null && ancientMasterShrine.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(caravan, ancientMasterShrine);
		}

		public override void Arrived(Caravan caravan)
		{
			this.ancientMasterShrine.Notify_CaravanArrived(caravan);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref ancientMasterShrine, "ancientMasterShrine", false);
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, AncientMasterShrineWO ancientMasterShrine)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() 
				=> CanVisit(caravan, ancientMasterShrine), 
				() => new CaravanArrivalAction_AncientMasterShrineWO(ancientMasterShrine), "VisitAncientMasterShrine".Translate(ancientMasterShrine.Label), 
				caravan, ancientMasterShrine.Tile, ancientMasterShrine, null);
		}


	}
}
