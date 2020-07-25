using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    class AncientMasterShrineWO : WorldObject
    {
		// todo label
        public void Notify_CaravanArrived(Caravan caravan)
        {
			Init(caravan);
			Find.WorldObjects.Remove(this);
		}

		protected void Init(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate ()
			{
				IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
				if (incidentParms.points < 400f)
				{
					incidentParms.points = 400f;
				}
				incidentParms.faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
				PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
				defaultPawnGroupMakerParms.generateFightersOnly = true;
				List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
				Map map = SetupCaravanAttackMap(caravan, list, false);
				MapInfo(map);
				if (list.Any<Pawn>())
				{
					LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, list);
				}
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				GlobalTargetInfo target = (!list.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
				Find.LetterStack.ReceiveLetter("Story_Shrine1_EnemyEncounterLetterLabel".Translate(), "Story_Shrine1_EnemyEncounterLetterMessage".Translate(), LetterDefOf.ThreatBig, target, null, null, null, null);
			}, "GeneratingMapForNewEncounter", false, null, true);
		}

		private void MapInfo(Map map)
        {
			// todo here or in AncientMasterShrineMP PostMapGenerate() ?
			Log.Message("1");
			if (map == null)
			{
				Log.Message($"I'm an idiot and map is null");
				return;
			}
			var shrines = map?.edificeGrid?.InnerArray;

			Log.Message("2");
			if (shrines == null) Log.Message($"The shrines is null");
			Log.Message($"count: {shrines.Length}");

			

			//var shrine = shrines.FirstOrDefault(x => !x.def?.building?.isNaturalRock ?? false);
			//if (shrine == null) Log.Message($"The shrine is null");

			Log.Message("3");
			if (map?.edificeGrid?.InnerArray == null)
			{
				Log.Message($"The array is null");
				return;
			}

			foreach (var building in map.edificeGrid.InnerArray.Where(x => x?.def != null).ToList())
			{
				//Log.Message($"Building details: " +
				//	$"\nname: {building?.def?.defName} " +
				//	$"\nlabel: {building?.Label}" +
				//	$"\nblueprint: {building?.def?.blueprintDef?.defName} " +
				//	$"\ncat: {building?.def?.category.ToString()}");

				if (building.def.defName == "AncientCryptosleepCasket")
                {
					var room = building.GetRoom();
					Log.Message($"Room cells: {room.CellCount}");
						
                }

				//Log.Message($"Comps: ");
				//foreach (var comp in building.AllComps)
				//{
				//	Log.Message($"{comp.ToString()}");
				//}

			}
		}

		public static Map SetupCaravanAttackMap(Caravan caravan, List<Pawn> enemies, bool sendLetterIfRelatedPawns)
		{
            int num = CaravanIncidentUtility.CalculateIncidentMapSize(caravan.PawnsListForReading, enemies);
			var size = new IntVec3(300, 0, 300);
			// Find.World.info.initialMapSize
			Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, Find.World.info.initialMapSize, CaravanStorySiteDefOf.AncientMasterShrineMP);
            IntVec3 playerStartingSpot;
			IntVec3 root;
			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerStartingSpot, out root);
			CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4), CaravanDropInventoryMode.DoNotDrop, true);
			for (int i = 0; i < enemies.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
				//GenSpawn.Spawn(enemies[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
			}
			if (sendLetterIfRelatedPawns)
			{
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(enemies, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
			}
			return map;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
			foreach (var o in base.GetFloatMenuOptions(caravan))
            {
				yield return o;
            }

			foreach (var own in CaravanArrivalAction_AncientMasterShrineWO.GetFloatMenuOptions(caravan, this))
            {
				yield return own;
            }
			yield break;
        }
    }
}
