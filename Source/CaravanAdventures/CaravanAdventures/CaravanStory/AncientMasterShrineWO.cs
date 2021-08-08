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
        private Room mainRoom = null;
        private int minMainRoomSize = 1500;
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
                Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, StoryUtility.GetAllowedMapSizeConcideringSettings(), CaravanStorySiteDefOf.CAAncientMasterShrineMP);
                mp = map.Parent as AncientMasterShrineMP;
                mainRoom = GetAncientShrineRooms(map).FirstOrDefault();

                if (mainRoom.CellCount > minMainRoomSize) //  && mainRoom.CellCount < map.AllCells.Count() / 2
                {
                    if (CompCache.StoryWC.GetCurrentShrineCounter() != CompCache.StoryWC.GetShrineMaxiumum) mp.boss = AddBoss(map, caravan, mainRoom);
                    else mp.lastJudgmentEntrance = InitCellarEntrace(map);
                    CompCache.StoryWC.wasShrineAmbushNoLuck = (mp.boss == null && mp.lastJudgmentEntrance == null);
                    DLog.Message($"Was shrine ambush: {CompCache.StoryWC.wasShrineAmbushNoLuck}");
                }
                else
                {
                    AddBandits(map, caravan);
                    CompCache.StoryWC.wasShrineAmbushNoLuck = true;
                }
                AdjustAncientShrines(map, caravan, mp.boss);

                //(Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4)
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;

                mp.Init();
            }, "Story_Shrine1_EnterPossibleShrine", true, null, false);
        }

        private void AddBandits(Map map, Caravan caravan, bool sendLetterIfRelatedPawns = true)
        {
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
            IntVec3 pos = default;
            if (!StoryUtility.CanSpawnSpotCloseToCaskets(mainRoom, map, out pos)) pos = mainRoom.Cells.Where(x => x.Walkable(map) && x.Standable(map)).RandomElement();

            var boss = StoryUtility.GetFittingMechBoss();
            GenSpawn.Spawn(boss, pos, map, WipeMode.Vanish);
            var compDormant = boss.TryGetComp<CompWakeUpDormant>();
            if (compDormant != null) compDormant.wakeUpIfColonistClose = true;

            return boss;
        }

        private void AdjustAncientShrines(Map map, Caravan caravan, Pawn boss = null)
        {
            var stateBackup = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;

            foreach (var room in GetAncientShrineRooms(map))
            {
                AddSpacers(room, map, caravan);
                AddMechanoidsToRoom(room, map, caravan, boss, RemoveHivesFromRoom(room));
                RemoveRewardsFromSmallShrines(room);
            }
            // we're killing insects as they can distract incoming help and have them march across the entire map while mechs destroy the player
            KillRemainingHivesAndInsectsOnMap(map);

            Current.ProgramState = stateBackup;
        }

        private void KillRemainingHivesAndInsectsOnMap(Map map)
        {
            foreach (var hive in map.spawnedThings.Where(thing => thing.def == ThingDefOf.Hive && thing.Faction == Faction.OfInsects).Reverse()) hive.Destroy();
            foreach (var insect in map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.Faction == Faction.OfInsects).Reverse()) insect.Destroy();
        }

        private bool RemoveHivesFromRoom(Room room)
        {
            var found = false;
            foreach (var thing in room.Regions.SelectMany(region => region.ListerThings.AllThings).Distinct().Reverse())
            {
                if (thing.def != ThingDefOf.Hive) continue;
                thing.Destroy();
                found = true;
            }
            return found;
        }

        private void RemoveRewardsFromSmallShrines(Room room)
        {
            if (room == mainRoom || room.CellCount > minMainRoomSize) return;
            var skipIdx = 0;
            foreach (var item in room.Regions.SelectMany(region => region.ListerThings.AllThings).Distinct().Reverse())
            {
                if (item.def.category != ThingCategory.Item || (item.def?.thingCategories != null && item.def.thingCategories.Contains(ThingCategoryDefOf.Chunks))) continue;
                if (skipIdx == 4)
                {
                    skipIdx = 0;
                    continue;
                }
                skipIdx++;
                item.Destroy();
            }
        }

        private void AddSpacers(Room room, Map map, Caravan caravan)
        {
            var casketCount = 0;
            if (room.CellCount > minMainRoomSize) casketCount = 6;
            //else if (room.CellCount > 240) casketCount = 4;
            //else if (room.CellCount > 120) casketCount = 2;
            else return;

            var casketGroupId = SetAndReturnCasketGroupId(room);

            CellRect rect;
            if (FindEmptyRectInRoom(room, 4, casketCount * 2 + 2, out rect) ||
                FindEmptyRectInRoom(room, casketCount * 2 + 2, 4, out rect))
            {
                CreateCasketsInRect(rect, room, map, caravan, casketGroupId);
            }

            if (room.CellCount < minMainRoomSize) return;

            if (FindEmptyRectInRoom(room, casketCount * 2 + 2, 4, out rect) ||
                FindEmptyRectInRoom(room, 4, casketCount * 2 + 2, out rect))
            {
                CreateCasketsInRect(rect, room, map, caravan, casketGroupId);
            }
        }

        private int SetAndReturnCasketGroupId(Room room)
        {
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
            var casket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket);
            casket.groupID = casketGroupId;
            GenSpawn.Spawn(casket, curCell, map, rotation, WipeMode.Vanish);
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile)
            {
                FixedMelanin = 0.1f,
                CanGeneratePawnRelations = false,
                ColonistRelationChanceFactor = 0,
                RelationWithExtraPawnChanceFactor = 0,
            });
            mp.generatedSoldiers.Add(pawn);
            casket.GetDirectlyHeldThings().TryAdd(pawn, true);
        }

        private bool FindEmptyRectInRoom(Room room, int width, int height, out CellRect rect)
        {
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

        private void AddMechanoidsToRoom(Room room, Map map, Caravan caravan, Pawn boss = null, bool removedHives = false)
        {
            var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
            incidentParms.faction = Faction.OfMechanoids;

            var mechPawnGroupMakerParams = CalculateMechPawnGroupMakerParams(room, map, caravan, removedHives, incidentParms);
            var spawnedMechs = PawnGroupMakerUtility.GeneratePawns(mechPawnGroupMakerParams, true).ToList();

            if (!spawnedMechs.Any()) return;

            var existingMechs = new List<Pawn>();
            foreach (var pawn in room.Regions.SelectMany(region => region.ListerThings.AllThings.OfType<Pawn>()))
            {
                if (pawn.Faction != Faction.OfMechanoids || existingMechs.Contains(pawn)) continue;
                var lord = pawn.GetLord();
                if (lord != null)
                {
                    lord.ownedPawns.Remove(pawn);
                    if (lord.ownedPawns.Count == 0)
                    {
                        DLog.Message($"lord with id {lord.loadID} has 0 owned pawns, removing");
                        map.lordManager.RemoveLord(lord);
                    }
                }
                existingMechs.Add(pawn);
            }

            //mp.generatedMechs.AddRange(spawnedMechs);
            var emptyCells = room.Cells.Where(x => x.Standable(map) && !x.Filled(map));
            var idx = 0;
            foreach (var cell in emptyCells.InRandomOrder().Take(spawnedMechs.Count))
            {
                var mech = spawnedMechs[idx++];
                GenSpawn.Spawn(mech, cell, map, WipeMode.Vanish);
                var compDormant = mech.TryGetComp<CompWakeUpDormant>();
                if (compDormant != null) compDormant.wakeUpIfColonistClose = true;
            }

            var combinedMechs = spawnedMechs.ToList();
            combinedMechs.AddRange(existingMechs);

            var newLord = LordMaker.MakeNewLord(incidentParms.faction, new LordJob_SleepThenAssaultColony(incidentParms.faction), map, combinedMechs);

            // LordJob_SleepThenMechanoidsDefend
            if (boss != null && room == mainRoom && !newLord.ownedPawns.Contains(boss)) newLord.AddPawn(boss);
            GenStep_SleepingMechanoids.SendMechanoidsToSleepImmediately(spawnedMechs);
        }

        private PawnGroupMakerParms CalculateMechPawnGroupMakerParams(Room room, Map map, Caravan caravan, bool removedHives, IncidentParms incidentParms)
        {
            var defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);

            DLog.Message($"Points before: {defaultPawnGroupMakerParms.points} roomcells: {room.CellCount}");

            var calcedFromRoomSize = Convert.ToInt32(defaultPawnGroupMakerParms.points 
                * (CompCache.StoryWC.GetCurrentShrineCounter() * ModSettings.shrineMechDifficultyMultiplier) 
                * ((room.CellCount > (map.AllCells.Count() / 2)
                    ? 3200
                    : room.CellCount >= minMainRoomSize 
                        ? Math.Max(room.CellCount, 3000) 
                        : room.CellCount) / 1000f));
            var minPoints = room.CellCount >= minMainRoomSize ? 2000 : 130;

            DLog.Message($"from roomsize: {calcedFromRoomSize} minpoints: {minPoints}");
            float selected = Math.Max(calcedFromRoomSize, minPoints);
            if (removedHives) selected += 200;
            selected = Math.Min(selected, room.CellCount >= minMainRoomSize 
                    ? ModSettings.maxShrineCombatPoints * 50f 
                    : ModSettings.maxShrineCombatPoints);

            var mechPawnGroupMakerParams = new PawnGroupMakerParms
            {
                groupKind = CaravanStory.StoryDefOf.CAMechanoidPawnGroupKindCombatMixed,
                tile = map.Tile,
                faction = Faction.OfMechanoids,
                points = selected
            };

            DLog.Message($"Points after: {mechPawnGroupMakerParams.points}");

            return mechPawnGroupMakerParams;
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

        private Thing InitCellarEntrace(Map map)
        {
            if (!StoryUtility.CanSpawnSpotCloseToCaskets(mainRoom, map, out var pos)) return null;

            var stateBackup = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;

            var light = ThingMaker.MakeThing(StoryDefOf.CAShrinePortal);
            var entrance = GenSpawn.Spawn(light, pos, map, WipeMode.Vanish);

            Current.ProgramState = stateBackup;
            return entrance;
        }

    }
}
