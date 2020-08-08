using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

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

		public static Faction EnsureSacrilegHunters()
        {
			var sacrilegHunters = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def.defName == "SacrilegHunters");
			if (sacrilegHunters == null)
			{
				sacrilegHunters = FactionGenerator.NewGeneratedFaction(DefDatabase<FactionDef>.GetNamedSilentFail("SacrilegHunters"));
				Find.FactionManager.Add(sacrilegHunters);
				var empireDef = FactionDefOf.Empire;
				empireDef.permanentEnemyToEveryoneExcept.Add(sacrilegHunters.def);
                Faction.Empire.TrySetNotHostileTo(sacrilegHunters);
            }
			// todo test relation being set correctly
			if (sacrilegHunters != null && Faction.OfPlayerSilentFail != null && sacrilegHunters.HostileTo(Faction.OfPlayerSilentFail)) sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Ally, goodwill = 100, other = Faction.OfPlayer });

			return sacrilegHunters;
		}

        internal static void CallBombardment(IntVec3 position, Map map, Pawn instigator)
        {
			Bombardment bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, position, map, WipeMode.Vanish);
			bombardment.impactAreaRadius = 7.9f;
			bombardment.explosionRadiusRange = new FloatRange(5.9f, 5.9f);
			bombardment.bombIntervalTicks = 60;
			bombardment.randomFireRadius = 1;
			bombardment.explosionCount = 6;
			bombardment.warmupTicks = 60;
			bombardment.instigator = instigator;
			SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera(null);
		}
	}
}
