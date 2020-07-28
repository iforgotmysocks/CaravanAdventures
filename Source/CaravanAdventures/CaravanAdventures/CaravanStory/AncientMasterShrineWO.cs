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

namespace CaravanAdventures.CaravanStory
{
    class AncientMasterShrineWO : WorldObject
    {
		// todo label
		// todo cleanup -> especially mechs 
		private Room mainRoom = null;
		private List<Pawn> generatedSoldiers = new List<Pawn>();
		private List<Pawn> generatedMechs = new List<Pawn>();
		private List<Pawn> generatedBandits = new List<Pawn>();

        public void Notify_CaravanArrived(Caravan caravan)
        {
			Init(caravan);
			Find.WorldObjects.Remove(this);
		}

		protected void Init(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate ()
			{
				Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, Find.World.info.initialMapSize, CaravanStorySiteDefOf.AncientMasterShrineMP);
				mainRoom = GetAncientShrineRooms(map).FirstOrDefault();
			
				AddEnemiesToRooms(map, caravan);
				
				if (mainRoom.CellCount > 1500) AddBoss(map, caravan, mainRoom);
				else AddBandits(map, caravan);

				//(Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4)
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true);
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			}, "GeneratingMapForNewEncounter", false, null, true);
		}

        private void AddBandits(Map map, Caravan caravan, bool sendLetterIfRelatedPawns = true)
        {
			// todo add notification about this being an ambush and not the real thing.

			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
			if (incidentParms.points < 400f)
			{
				incidentParms.points = 400f;
			}
			incidentParms.faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
			defaultPawnGroupMakerParms.generateFightersOnly = true;
			generatedBandits = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();

			IntVec3 playerStartingSpot;
			IntVec3 root;

			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerStartingSpot, out root);
			for (int i = 0; i < generatedBandits.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
				GenSpawn.Spawn(generatedBandits[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
			}
			if (sendLetterIfRelatedPawns)
			{
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(generatedBandits, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
			}

			if (generatedBandits.Any()) LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, generatedBandits);
			GlobalTargetInfo target = (!generatedBandits.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(generatedBandits[0].Position, map, false);
			Find.LetterStack.ReceiveLetter("Story_Shrine1_EnemyEncounterLetterLabel".Translate(), "Story_Shrine1_EnemyEncounterLetterMessage".Translate(), LetterDefOf.ThreatBig, target, null, null, null, null);

		}

		private void AddBoss(Map map, Caravan caravan, Room mainRoom)
        {
			// todo add notification about this being the real thing
			// todo add Boss?
        }

        private void AddEnemiesToRooms(Map map, Caravan caravan)
        {
			var stateBackup = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;
			
			foreach (var room in GetAncientShrineRooms(map))
            {
				AddSpacers(room, map, caravan);
				AddMechanoidsToRoom(room, map, caravan);
            }
			
			Current.ProgramState = stateBackup;
		}

        private void AddSpacers(Room room, Map map, Caravan caravan)
        {
			var casketCount = 0;
			if (room.CellCount > 1500) casketCount = 6;
			else if (room.CellCount > 240) casketCount = 4;
			else if (room.CellCount > 120) casketCount = 2;
			else return;

			var casketGroupId = SetAndReturnCasketGroupId(room);

			Log.Message($"Casket id {casketGroupId} for room id {room.ID}");

			CellRect rect;
			if (FindEmptyRectInRoom(room, 4, casketCount * 2 + 2, out rect) ||
				FindEmptyRectInRoom(room, casketCount * 2 + 2, 4, out rect))
			{
				CreateCasketsInRect(rect, room, map, caravan, casketGroupId);
			}

			if (room.CellCount < 1500) return;

			if (FindEmptyRectInRoom(room, casketCount * 2 + 2, 4, out rect) ||
				FindEmptyRectInRoom(room, 4, casketCount * 2 + 2, out rect))
			{
				CreateCasketsInRect(rect, room, map, caravan, casketGroupId);
			}
		}

        private int SetAndReturnCasketGroupId(Room room)
        {
			var id = Rand.Range(1, Int32.MaxValue - 1);
			var caskets = room.ContainedThings(ThingDefOf.AncientCryptosleepCasket);
			foreach (var casket in caskets)
			{
				var casketBuilding = casket as Building_AncientCryptosleepCasket;
				casketBuilding.groupID = id;
				Log.Message($"Setting casket to id: {id}");
			}
			return id;
		}

		private void CreateCasketsInRect(CellRect rect, Room room, Map map, Caravan caravan, int casketGroupId)
		{

			if (rect.Width > rect.Height)
			{
				var bottomMargin = 0;
				var leftMargin = 0;
				for (var i = rect.minX; i <= rect.maxX; i++)
				{
					for (var j = rect.minZ; j <= rect.maxZ; j++)
					{
						var curCell = new IntVec3(i, 0, j);

						if (leftMargin != 0 && leftMargin % 2 == 0 && bottomMargin == 1)
						{
							SpawnCasketAt(curCell, map, caravan, Rot4.South, casketGroupId);
						}
						//else GenDebugTorch(curCell, map);

						bottomMargin++;
					}
					leftMargin++;
					bottomMargin = 0;
				}
			}
			else
			{
				var bottomMargin = 0;
				var leftMargin = 0;
				for (var i = rect.minZ; i <= rect.maxZ; i++)
				{
					for (var j = rect.minX; j <= rect.maxX; j++)
					{
						var curCell = new IntVec3(j, 0, i);

						if (bottomMargin != 0 && bottomMargin % 2 == 0 && leftMargin == 1)
						{
							SpawnCasketAt(curCell, map, caravan, Rot4.East, casketGroupId);
						}
						//else GenDebugTorch(curCell, map);

						leftMargin++;
					}
					bottomMargin++;
					leftMargin = 0;
				}

				//foreach (var cell in rect.Cells)
				//{
				//	GenDebugTorch(cell, map);
				//}
			}
		}

        private void GenDebugTorch(IntVec3 cell, Map map)
        {
			try
			{
				var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);
				GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
			}
			catch (Exception e)
			{
				// todo figure out what's throwing the error.
				Log.Warning(e.ToString());
			}
		}

        private void SpawnCasketAt(IntVec3 curCell, Map map, Caravan caravan, Rot4 rotation, int casketGroupId)
        {
            try
            {
				// todo if casket is opend, all caskets need to trigger!
				var casket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket);
				casket.groupID = casketGroupId;
				GenSpawn.Spawn(casket, curCell, map, rotation, WipeMode.Vanish);
				var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile));
				generatedSoldiers.Add(pawn);
				var addedPawn = casket.GetDirectlyHeldThings().TryAdd(pawn, true);
				Log.Message($"Was able to add pawn to casket: {addedPawn}");
			}
			catch (Exception e)
            {
				Log.Warning(e.ToString());
            }
        }

        private bool FindEmptyRectInRoom(Room room, int width, int height, out CellRect rect)
        {
			// todo improve 
			// --> reduce rect to find fitting place if unsuccessful
			// --> check what things block cells and remove things that don't matter
			rect = new CellRect(0, 0, width, height);
			foreach (var cell in room.Cells)
            {
				rect = new CellRect(cell.x, cell.z, width, height);
				var valid = true;
				foreach (var rectCell in rect.Cells)
                {
					if (!room.ContainsCell(rectCell) || !rectCell.Standable(room.Map) || rectCell.Filled(room.Map))
					{
						valid = false;
						break;
					}
                }
				if (valid) return true;
            }
			return false;
        }

        private IEnumerable<Room> GetAncientShrineRooms(Map map) => map.spawnedThings.Where(x => x.def.defName == "AncientCryptosleepCasket").GroupBy(casket => casket.GetRoom().ID).Select(r => r.First().GetRoom()).OrderByDescending(order => order.CellCount);

        private void AddMechanoidsToRoom(Room room, Map map, Caravan caravan)
        {
			//todo test this appraoch? 
			//CellRect around;
			//IntVec3 near;
			//if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out around, out near, map))

			// todo calculte room quaters and generate multiple groups


			// todo remove hives and increase points to make up for missing hives
			var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
			incidentParms.faction = Faction.OfMechanoids;
			var defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);

			Log.Message($"Points before: {defaultPawnGroupMakerParms.points} roomcells: {room.CellCount}");
			var mechPawnGroupMakerParams = new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = Faction.OfMechanoids,
				points = Math.Max(120, Convert.ToInt32(defaultPawnGroupMakerParms.points * new IntRange(1, 1).RandomInRange * (room.CellCount / 100f)))
			};
			
			Log.Message($"Points after: {mechPawnGroupMakerParams.points}");
			generatedMechs = PawnGroupMakerUtility.GeneratePawns(mechPawnGroupMakerParams, true).ToList();
			
			var emptyCells = room.Cells.Where(x => x.Standable(map) && !x.Filled(map));
			var idx = 0;
			foreach (var cell in emptyCells.InRandomOrder().Take(generatedMechs.Count))
            {
				var mech = generatedMechs[idx++];
				GenSpawn.Spawn(mech, cell, map, WipeMode.Vanish);
				var compDormant = mech.TryGetComp<CompWakeUpDormant>();
				if (compDormant != null) compDormant.wakeUpIfColonistClose = true;
            }
			if (generatedMechs.Any()) LordMaker.MakeNewLord(incidentParms.faction, new LordJob_SleepThenAssaultColony(incidentParms.faction), map, generatedMechs);
			GenStep_SleepingMechanoids.SendMechanoidsToSleepImmediately(generatedMechs);
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
