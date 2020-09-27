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
	class AncientMasterShrineWO : WorldObject
	{
		// todo label
		// todo cleanup -> especially mechs 
		private Room mainRoom = null;
		private AncientMasterShrineMP mp = null;

		public void Notify_CaravanArrived(Caravan caravan)
		{
			Init(caravan);
			Find.WorldObjects.Remove(this);
		}

		protected void Init(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate ()
			{
				Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, Find.World.info.initialMapSize, CaravanStorySiteDefOf.CAAncientMasterShrineMP);
				mp = map.Parent as AncientMasterShrineMP;
				mainRoom = GetAncientShrineRooms(map).FirstOrDefault();

                // todo check mainroom size depending on map size?
                if (mainRoom.CellCount > 1500) mp.boss = AddBoss(map, caravan, mainRoom);
                else AddBandits(map, caravan);

				AddEnemiesToRooms(map, caravan, mp.boss);

				//(Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4)
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true);
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;

				mp.Init();
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
			mp.generatedBandits = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
            MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out _, out var root);
			for (int i = 0; i < mp.generatedBandits.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
				GenSpawn.Spawn(mp.generatedBandits[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
			}
			if (sendLetterIfRelatedPawns)
			{
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(mp.generatedBandits, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
			}

			if (mp.generatedBandits.Any()) LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(incidentParms.faction, true, true, false, false, true), map, mp.generatedBandits);
			GlobalTargetInfo target = (!mp.generatedBandits.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(mp.generatedBandits[0].Position, map, false);
			Find.LetterStack.ReceiveLetter("Story_Shrine1_EnemyEncounterLetterLabel".Translate(), "Story_Shrine1_EnemyEncounterLetterMessage".Translate(), LetterDefOf.ThreatBig, target, null, null, null, null);
		}

		private Pawn AddBoss(Map map, Caravan caravan, Room mainRoom)
		{
			// todo boss not attackign with group -> find error
			// todo map gen can also fail on just not spawning caskets and therefore no mechs, if that happens, the boss can't be spawned!
			IntVec3 pos = default;
			if (!StoryUtility.CanSpawnSpotCloseToCaskets(mainRoom, map, out pos)) return null;
			
			// todo get random boss def with fitting implant
			//var boss = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamedSilentFail("CABossMechDevourer"), Faction.OfMechanoids);
			//boss.health.AddHediff(HediffDef.Named("EXT1Basic"), boss.health.hediffSet.GetBrain());
			var boss = StoryUtility.GetFittingMechBoss();
			GenSpawn.Spawn(boss, pos, map, WipeMode.Vanish);
			var compDormant = boss.TryGetComp<CompWakeUpDormant>();
			if (compDormant != null) compDormant.wakeUpIfColonistClose = true;
			//GenStep_SleepingMechanoids.SendMechanoidsToSleepImmediately(new List<Pawn> { boss });
			return boss;
		}

		private void AddEnemiesToRooms(Map map, Caravan caravan, Pawn boss = null)
		{
			var stateBackup = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;

			foreach (var room in GetAncientShrineRooms(map))
			{
				AddSpacers(room, map, caravan);
				AddMechanoidsToRoom(room, map, caravan, boss);
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
			// todo test id
			//var id = Rand.Range(1, Int32.MaxValue - 1);
			var id = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
			var caskets = room.ContainedThings(ThingDefOf.AncientCryptosleepCasket);
			foreach (var casket in caskets)
			{
				var casketBuilding = casket as Building_AncientCryptosleepCasket;
				casketBuilding.groupID = id;
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
						leftMargin++;
					}
					bottomMargin++;
					leftMargin = 0;
				}
			}
		}

		private void SpawnCasketAt(IntVec3 curCell, Map map, Caravan caravan, Rot4 rotation, int casketGroupId)
		{
			// todo if casket is opend, all caskets need to trigger!
			var casket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket);
			casket.groupID = casketGroupId;
			GenSpawn.Spawn(casket, curCell, map, rotation, WipeMode.Vanish);
			var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile));
			mp.generatedSoldiers.Add(pawn);
			casket.GetDirectlyHeldThings().TryAdd(pawn, true);
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

		private void AddMechanoidsToRoom(Room room, Map map, Caravan caravan, Pawn boss = null)
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
			var spawnedMechs = PawnGroupMakerUtility.GeneratePawns(mechPawnGroupMakerParams, true).ToList();
			if (!spawnedMechs.Any()) return;
			mp.generatedMechs.AddRange(spawnedMechs);
			var emptyCells = room.Cells.Where(x => x.Standable(map) && !x.Filled(map));
			var idx = 0;
			foreach (var cell in emptyCells.InRandomOrder().Take(spawnedMechs.Count))
			{
				var mech = spawnedMechs[idx++];
				GenSpawn.Spawn(mech, cell, map, WipeMode.Vanish);
				var compDormant = mech.TryGetComp<CompWakeUpDormant>();
				if (compDormant != null) compDormant.wakeUpIfColonistClose = true;
			}
			
			// LordJob_SleepThenMechanoidsDefend
			if (boss != null && room == mainRoom)
			{
				spawnedMechs.Add(boss);
				Log.Message($"Adding boss to list");
			}
			LordMaker.MakeNewLord(incidentParms.faction, new LordJob_SleepThenAssaultColony(incidentParms.faction), map, spawnedMechs);
			GenStep_SleepingMechanoids.SendMechanoidsToSleepImmediately(spawnedMechs);
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (var baseOption in base.GetFloatMenuOptions(caravan))
			{
				yield return baseOption;
			}

			foreach (var own in CaravanArrivalAction_AncientMasterShrineWO.GetFloatMenuOptions(caravan, this))
			{
				yield return own;
			}
			yield break;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (var baseGiz in base.GetGizmos())
			{
				yield return baseGiz;
			}

			if (Find.WorldSelector.SingleSelectedObject == this)
			{
				var giveUpCommand = new Command_Action
				{
					defaultLabel = "GiveUpOnClueLabel".Translate(),
					defaultDesc = "GiveUpOnClueDesc".Translate(),
					order = 198f,
					icon = ContentFinder<Texture2D>.Get("UI/commands/AbandonHome", true),
					action = () =>
					{
						// todo cleanup + notify story to tick on
						SoundDefOf.Click.PlayOneShot(null);
						CompCache.StoryWC.ResetCurrentShrineFlags();
						this.Destroy();
					}
				};

				yield return giveUpCommand;

			}
		}

    }
}
