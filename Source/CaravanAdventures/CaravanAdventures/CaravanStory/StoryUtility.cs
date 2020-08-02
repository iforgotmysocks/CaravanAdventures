using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    static class StoryUtility
    {
        public static bool CanSpawnSpotCloseToCaskets(Room mainRoom, Map map, out IntVec3 pos)
        {
			var casket = mainRoom.ContainedThings(ThingDefOf.AncientCryptosleepCasket).RandomElement();
			pos = default;
			if (casket != null)
			{
				for (int i = 0; i < 50; i++)
				{
					CellFinder.TryFindRandomSpawnCellForPawnNear_NewTmp(casket.Position, map, out var result, 4);
					if (mainRoom.Cells.Contains(result))
					{
						pos = result;
						return true;
					}
				}
			}
			else
			{
				//pos = mainRoom.Cells.Where(x => x.Standable(map) && !x.Filled(map)).InRandomOrder().FirstOrDefault();
				return false;
			}

			return false;
		}

        internal static Pawn GetGiftedPawn() => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null);
    }
}
