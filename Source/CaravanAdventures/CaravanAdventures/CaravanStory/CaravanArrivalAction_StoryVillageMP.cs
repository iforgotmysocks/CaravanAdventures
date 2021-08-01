using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class CaravanArrivalAction_StoryVillageMP : CaravanArrivalAction
    {
		public override string Label => "VisitStoryVillageLabel".Translate(storyVillageMP.Label);
		public override string ReportString => "CaravanVisiting".Translate(storyVillageMP.Label);
		private StoryVillageMP storyVillageMP;

		public CaravanArrivalAction_StoryVillageMP()
		{
		}

		public CaravanArrivalAction_StoryVillageMP(StoryVillageMP storyVillageMP)
		{
			this.storyVillageMP = storyVillageMP;
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, StoryVillageMP storyVillageMP)
		{
            if ((CompCache.StoryWC.storyFlags["IntroVillage_PlayerWon"] 
				|| CompCache.StoryWC.storyFlags["IntroVillage_Finished"])
                 && (storyVillageMP?.Map == null
                     || (storyVillageMP?.Map != null
                         && !storyVillageMP.Map.mapPawns.FreeColonists.Any(x => !x.Dead)))) return false;

            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters)) return FloatMenuAcceptanceReport.WithFailMessage("StoryVillageCantArriveHostile".Translate(StoryUtility.FactionOfSacrilegHunters.NameColored));
			return storyVillageMP != null && storyVillageMP.Spawned;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!floatMenuAcceptanceReport) return floatMenuAcceptanceReport;
			if (this.storyVillageMP != null && storyVillageMP.Tile != destinationTile) return false;
			return CanVisit(caravan, storyVillageMP);
		}

		public override void Arrived(Caravan caravan)
		{
			if (!CanVisit(caravan, storyVillageMP)) return;
			this.storyVillageMP.Notify_CaravanArrived(caravan);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref storyVillageMP, "storyVillageMP", false);
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, StoryVillageMP storyVillage)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() 
				=> CanVisit(caravan, storyVillage), 
				() => new CaravanArrivalAction_StoryVillageMP(storyVillage), "VisitStoryVillageLabel".Translate(storyVillage.Label), 
				caravan, storyVillage.Tile, storyVillage, null);
		}

	}
}
