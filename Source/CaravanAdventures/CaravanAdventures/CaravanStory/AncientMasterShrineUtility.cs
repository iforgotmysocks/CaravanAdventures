using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    static class AncientMasterShrineUtility
    {
        public static IntVec3 GetSpawnSpotCloseToCaskets(Room mainRoom, Map map)
        {
			var casket = mainRoom.ContainedThings(ThingDefOf.AncientCryptosleepCasket).RandomElement();
			IntVec3 pos = default;
			if (casket != null)
			{
				for (int i = 0; i < 50; i++)
				{
					CellFinder.TryFindRandomSpawnCellForPawnNear_NewTmp(casket.Position, map, out var result, 4);
					if (mainRoom.Cells.Contains(result)) return result;
				}
			}
			else
			{
				pos = mainRoom.Cells.Where(x => x.Standable(map) && !x.Filled(map)).InRandomOrder().FirstOrDefault();
				return pos;
			}

			return pos;
		}

    }
}
