﻿using CaravanAdventures.CaravanStory.Lords;
using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    class StoryVillageMP : Settlement
    {
        private int ticks;
        private float ticksTillReinforcements = -1f;
        private float timerTillRemoval = -1f;
        private float timerForceStartRaid = -1f;
        private bool sacHuntersFleeing;
        private bool sacHuntersCiviliansFleeing;
        private bool mainCharLeftOrDied;
        private IntVec3 centerPoint;

        public StoryVillageMP()
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref sacHuntersFleeing, "sacHuntersFleeing");
            Scribe_Values.Look(ref sacHuntersCiviliansFleeing, "sacHuntersCiviliansFleeing");
            Scribe_Values.Look(ref mainCharLeftOrDied, "mainCharLeftOrDied");
            Scribe_Values.Look(ref centerPoint, "centerPoint");
            Scribe_Values.Look(ref ticksTillReinforcements, "ticksTillReinforcements", -1f);
            Scribe_Values.Look(ref timerTillRemoval, "timerTillRemoval", -1f);
            Scribe_Values.Look(ref timerForceStartRaid, "timerForceStartRaid", -1f);
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, StoryUtility.GetAllowedMapSizeConcideringSettings(), null);
            }, "GeneratingMap", false, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                StoryUtility.GenerateStoryContact();
                var storyChar = CompCache.StoryWC.questCont.Village?.StoryContact;
                //var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, null);
                var label = "StoryVillageArrivedLetterTitle".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID()));
                var text = "StoryVillageArrivedLetterMessage".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())).CapitalizeFirst();

                //var storyContactCell = CellFinder.RandomNotEdgeCell(Math.Min(orGenerateMap.Size.x / 2 - (orGenerateMap.Size.x / 6), orGenerateMap.Size.z / 2 - (orGenerateMap.Size.y / 6)), Map);

                if (storyChar == null)
                {
                    Log.Warning($"storychar was null");
                }

                if (!CompCache.StoryWC.storyFlags["IntroVillage_Entered"])
                {
                    if (!CellFinder.TryFindRandomSpawnCellForPawnNear(new IntVec3(Map.Size.x / 2, 0, Map.Size.z / 2), Map, out var storyContactCell, 8, x => x.Standable(Map)))
                    {
                        Log.Warning("Couldn't find a cell to spawn pawn");
                    }

                    if (storyChar != null)
                    {
                        if (storyChar.Spawned) storyChar.DeSpawn();
                        Helper.RunSafely(() => StoryUtility.FreshenUpPawn(storyChar));
                        GenSpawn.Spawn(storyChar, storyContactCell, Map);
                        StoryUtility.AssignDialog("StoryVillage_Conversation", storyChar, GetType().ToString(), "ConversationFinished");
                        AddNewLordAndAssignStoryChar(storyChar);
                    }
                    CompCache.StoryWC.SetSF("IntroVillage_Entered");
                    timerForceStartRaid = 60000;
                }

                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, Faction, null, null, null);
                CaravanEnterMapUtility.Enter(caravan, Map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;

            }, "StoryVillageEnterMapMessage", false, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
        }

        private void AddNewLordAndAssignStoryChar(Pawn storyChar)
        {
            centerPoint = StoryUtility.GetCenterOfSettlementBase(Map, StoryUtility.FactionOfSacrilegHunters, false);
            var raidLords = Map.lordManager.lords.Where(lord => lord.faction == StoryUtility.FactionOfSacrilegHunters);
            DLog.Message($"hunter lords: {raidLords.Select(lord => lord.ownedPawns).Count()}");

            if (Helper.ExpRM && DefDatabase<FactionDef>.AllDefsListForReading.FirstOrDefault(x => Helper.Exp.expSettingsDef.primaryEnemyFactionDef == x) == null) StoryUtility.EnsureEvilHostileFactionForExpansion(true);
            else if (!Helper.ExpRM && Faction.OfMechanoids == null) StoryUtility.EnsureEvilMechanoidFaction(FactionRelationKind.Hostile);

            foreach (var lord in raidLords.Reverse())
            {
                var pawnsToReassign = lord.ownedPawns;
                lord.lordManager.RemoveLord(lord);
                if (centerPoint == default && pawnsToReassign.Count != 0) centerPoint = pawnsToReassign.FirstOrDefault().Position;
                var genLord = LordMaker.MakeNewLord(Faction, new LordJob_DefendBaseAgaintHostiles(Faction, centerPoint), Map, pawnsToReassign);
                DLog.Message($"Created lord with id {genLord.loadID}");
            }

            var selLord = raidLords.OrderByDescending(x => x.ownedPawns.Count).FirstOrDefault();
            if (storyChar.GetLord() != null && storyChar.GetLord() != selLord) storyChar.GetLord().ownedPawns.Remove(storyChar);
            if (storyChar.GetLord() == null) selLord.AddPawn(storyChar);

            DLog.Message($"After applying lord, story char has lord with id: {storyChar.GetLord().loadID}");
        }

        public override void PostMapGenerate()
        {
            // override without calling base to avoid error of not existing timed raid component
        }

        public void ConversationFinished(Pawn initiator, Pawn addressed)
        {
            DLog.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            var diaNode4 = new DiaNode("StoryVillage_Dia1_4".Translate(addressed.NameShortColored));
            diaNode4.options.Add(new DiaOption("StoryVillage_Dia1_4_Option1".Translate()) { action = () => SpawnMechArmy(), resolveTree = true });

            var diaNode3 = new DiaNode("StoryVillage_Dia1_3".Translate(addressed.NameShortColored));
            diaNode3.options.Add(new DiaOption("StoryVillage_Dia1_3_Option1".Translate()) { link = diaNode4 });

            var diaNode2 = new DiaNode("StoryVillage_Dia1_2".Translate(addressed.NameShortColored));
            diaNode2.options.Add(new DiaOption("StoryVillage_Dia1_2_Option1".Translate()) { link = diaNode3 });
            diaNode2.options.Add(new DiaOption("StoryVillage_Dia1_2_Option2".Translate()) { link = diaNode3 });

            diaNode = new DiaNode("StoryVillage_Dia1_1".Translate(initiator.NameShortColored));
            diaNode.options.Add(new DiaOption("StoryVillage_Dia1_1_Option1".Translate()) { link = diaNode2 }); ;

            TaggedString taggedString = "StoryVillage_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void SpawnMechArmy(bool pawnWasAlreadyDead = false, bool pawnAlreadyLeft = false)
        {
            if (CompCache.StoryWC.storyFlags["IntroVillage_MechsArrived"]) return;
            var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;
            if (!pawnWasAlreadyDead && !pawnAlreadyLeft)
            {
                CompCache.StoryWC.SetSF("IntroVillage_TalkedToFriend");
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrived".Translate(storyChar.NameShortColored, GenderUtility.GetPossessive(storyChar.gender)));
                ticksTillReinforcements = 60 * 15;
            }
            else if (pawnWasAlreadyDead)
            {
                CompCache.StoryWC.SetSF("IntroVillage_FriendAlreadyDeadOrLeft");
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrivedFriendAlreadyDead".Translate(storyChar.NameShortColored, GenderUtility.GetPossessive(storyChar.gender)));
                timerTillRemoval = 60 * 60 * 2;
            }
            else if (pawnAlreadyLeft)
            {
                CompCache.StoryWC.SetSF("IntroVillage_FriendAlreadyDeadOrLeft");
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrivedFriendDiedFleeing".Translate(storyChar.NameShortColored, GenderUtility.GetPossessive(storyChar.gender)));
                timerTillRemoval = 60 * 60 * 2;
            }

            var incidentParms = new IncidentParms
            {
                target = Map,
                //incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
                points = StoryUtility.GetIncPoints(32000f),
                faction = Helper.ExpRMNewFaction,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                customLetterDef = LetterDefOf.ThreatBig,
                infestationLocOverride = Helper.ExpRM ? (IntVec3?)centerPoint : null
            };
            DLog.Message($"Default threat points: {StorytellerUtility.DefaultThreatPointsNow(incidentParms.target)}");
            if (Helper.RunSafely(() => Helper.ExpRM ? StoryDefOf.CAUnusualInfestation.Worker.TryExecute(incidentParms) : StoryDefOf.CAMechRaidMixed.Worker.TryExecute(incidentParms)) != true)
            {
                Log.Error($"Creating CA mech raid failed due to some incompatibility, error above.");
            };

            CompCache.StoryWC.SetSF("IntroVillage_MechsArrived");

            var comp = GetComponent<TimedDetectionPatrols>();
            comp.Init(Helper.ExpRMNewFaction);
            comp.StartDetectionCountdown(30000, -1, (int)StoryUtility.GetIncPoints(8000, 5000));
        }

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.CAStoryVillageMG;

        public override void Tick()
        {
            if (this.trader != null) this.trader.TraderTrackerTick();
            for (int i = 0; i < AllComps.Count; i++)
            {
                AllComps[i].CompTick();
            }

            if (base.HasMap)
            {
                if (ticks >= 60)
                {
                    //CheckShouldSacHuntersFlee();
                    CheckMainStoryCharDiedOrLeft();
                    CheckPlayerLeftAndAbandon();
                    ticks = 0;
                }
                if (ticksTillReinforcements == 0)
                {
                    StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map, StoryUtility.GetIncPoints(6500, custDevider: 32), StoryUtility.GetIncPoints(7000, custDevider: 32), centerPoint);
                    CompCache.StoryWC.SetSF("IntroVillage_ReinforcementsArrived");
                    ReinforcementConvo();
                    CheckShouldCiviliansFlee();
                }
                if (timerForceStartRaid == 0 && !CompCache.StoryWC.storyFlags["IntroVillage_MechsArrived"])
                {
                    SpawnMechArmy();
                    timerTillRemoval = 60 * 60 * 2;
                }

                ticks++;
                ticksTillReinforcements--;
                timerTillRemoval--;
                timerForceStartRaid--;
            }
        }

        private void CheckMainStoryCharDiedOrLeft()
        {
            if (mainCharLeftOrDied) return;
            var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;

            if (storyChar == null)
            {
                Log.Error($"story char was null, continueing quest as if the storyChar already died");
                SpawnMechArmy(true, false);
                mainCharLeftOrDied = true;
                return;
            }
            else if (!CompCache.StoryWC.storyFlags["IntroVillage_TalkedToFriend"] && storyChar.Dead)
            {
                SpawnMechArmy(true, false);
                mainCharLeftOrDied = true;
                return;
            }
            else if (!CompCache.StoryWC.storyFlags["IntroVillage_TalkedToFriend"] && !Map.mapPawns.AllPawnsSpawned.Contains(storyChar))
            {
                SpawnMechArmy(false, true);
                mainCharLeftOrDied = true;
                return;
            }

            if (!CompCache.StoryWC.storyFlags["IntroVillage_MechsArrived"]) return;
            // todo check if i should add .Destroyed or if .Dead is enough if the body is fully removed
            if (Map.mapPawns.AllPawnsSpawned.Contains(storyChar) && !storyChar.Dead && !storyChar.Downed) return;

            DiaNode diaNode = null;
            diaNode = new DiaNode(storyChar.Dead || storyChar.Downed
                ? "StoryVillage_Dia3_1_Dying".Translate()
                : Map.mapPawns.FreeColonistsSpawnedCount != 0
                    ? "StoryVillage_Dia3_1_Alive".Translate()
                    : "StoryVillage_Dia3_1_AliveNoHelpFromPlayer".Translate());
            diaNode.options.Add(new DiaOption("StoryVillage_Dia3_1_Option1".Translate()) { resolveTree = true }); ;

            TaggedString taggedString = "StoryVillage_Dia3_Title".Translate(storyChar.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
            mainCharLeftOrDied = true;

            if (!storyChar.Dead && !storyChar.Downed) CompCache.StoryWC.SetSF("IntroVillage_PlayerWon");

            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, storyChar.Dead || storyChar.Downed ? "StoryVillage_QuestUpdate_CanLeaveFailed".Translate() : "StoryVillage_QuestUpdate_CanLeaveSuccess".Translate(), false, true);
        }

        private void CheckShouldSacHuntersFlee()
        {
            // currently disabled as hunters flee on their own, also didn't work correctly yet, hunters tried to leave at... weird times (when all were dead for example)
            if (sacHuntersFleeing || !HasMap) return;
            var huntersAll = Map.mapPawns.AllPawnsSpawned.Where(x => x.Faction == StoryUtility.FactionOfSacrilegHunters);
            var deadOrDowned = huntersAll.Where(x => x.Dead || x.Downed).Count();
            if (deadOrDowned < huntersAll.Count() * 0.8) return;

            var raidLords = Map.lordManager.lords.Where(lord => lord.faction == StoryUtility.FactionOfSacrilegHunters);
            var hunters = new List<Pawn>();
            foreach (var lord in raidLords.Reverse())
            {
                hunters.AddRange(lord.ownedPawns);
                lord.lordManager.RemoveLord(lord);
            }

            Messages.Message(new Message("StoryVillage_SacHunters_Fleeing".Translate(), MessageTypeDefOf.NegativeEvent));
            LordMaker.MakeNewLord(Faction, new LordJob_ExitMapBest(LocomotionUrgency.Sprint, false, false), Map, hunters);
            sacHuntersFleeing = true;
        }

        private void CheckShouldCiviliansFlee()
        {
            if (sacHuntersCiviliansFleeing || !HasMap) return;
            var civs = Map.mapPawns.AllPawnsSpawned.Where(x => x.Faction == StoryUtility.FactionOfSacrilegHunters
                && (x.kindDef != StoryDefOf.CASacrilegHunters_ExperiencedHunter
                && x.kindDef != StoryDefOf.CASacrilegHunters_ExperiencedHunterVillage
                && x.kindDef != StoryDefOf.CASacrilegHunters_Hunter
                && x.kindDef != StoryDefOf.CASacrilegHunters_HunterVillage)
                || x == CompCache.StoryWC.questCont.Village.StoryContact);

            foreach (var civ in civs)
            {
                var lord = civ.GetLord();
                if (lord != null) lord.ownedPawns.Remove(civ);
                if (lord != null && lord.ownedPawns.Count == 0) Map.lordManager.RemoveLord(lord);
            }

            Messages.Message(new Message("StoryVillage_SacHuntersCivs_Fleeing".Translate(), MessageTypeDefOf.NegativeEvent));
            LordMaker.MakeNewLord(Faction, new LordJob_ExitMapBest(LocomotionUrgency.Jog, false, false), Map, civs);
            sacHuntersCiviliansFleeing = true;
        }

        private void CheckPlayerLeftAndAbandon()
        {
            if (!CompCache.StoryWC.storyFlags["IntroVillage_MechsArrived"] || !HasMap || !mainCharLeftOrDied) return;
            // change to remove when downed?
            if (timerTillRemoval > 0) return;
            //if (Map.mapPawns.FreeColonistsSpawned.Any(x => !x.Dead)) return;
            if (Map.mapPawns.AnyPawnBlockingMapRemoval) return;
            var killCamp = Map.mapPawns.AllPawnsSpawned.Any(x => x.Faction == Faction.OfMechanoids && !x.Dead && !x.Downed);
            Current.Game.DeinitAndRemoveMap(Map, false);

            if (killCamp)
            {
                DLog.Message($"camp destroyed");
                this.Destroy();
                var worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement) as MapParent;
                worldObject.Tile = this.Tile;
                worldObject.SetFaction(this.Faction);
                Find.WorldObjects.Add(worldObject);
                CompCache.StoryWC.questCont.Village.DestroyedSettlement = worldObject;
            }

            StoryUtility.AdjustGoodWill(75);
            CompCache.StoryWC.SetSF("IntroVillage_Finished");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_Survived".Translate());
            Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_StoryVillage_Arrival, true, CompCache.StoryWC.storyFlags["IntroVillage_PlayerWon"] ? QuestEndOutcome.Success : QuestEndOutcome.Fail);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            // -> handled in CheckPlayerLeftAndAbandon()
            alsoRemoveWorldObject = false;
            return false;
        }

        public void ReinforcementConvo()
        {
            DiaNode diaNode = null;
            diaNode = new DiaNode("StoryVillage_Dia2_1".Translate());
            diaNode.options.Add(new DiaOption("StoryVillage_Dia2_1_Option1".Translate()) { resolveTree = true });

            TaggedString taggedString = "StoryVillage_Dia2_Title".Translate(CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (var storyVillageOption in CaravanArrivalAction_StoryVillageMP.GetFloatMenuOptions(caravan, this))
            {
                yield return storyVillageOption;
            }

            foreach (var baseOptions in base.GetFloatMenuOptions(caravan))
            {
                yield return baseOptions;
            }

            yield break;
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            foreach (var baseGiz in base.GetCaravanGizmos(caravan)) yield return baseGiz;

            if (CaravanArrivalAction_StoryVillageMP.CanVisit(caravan, this))
            {
                yield return new Command_Action
                {
                    icon = Patches.TexCustom.Drop,
                    defaultLabel = "VisitStoryVillageLabel".Translate(Label),
                    defaultDesc = "CaravanVisiting".Translate(Label),
                    action = delegate ()
                    {
                        Notify_CaravanArrived(caravan);
                    }
                };
            }
        }

    }
}
