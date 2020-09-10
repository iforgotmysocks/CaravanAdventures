using CaravanAdventures.CaravanStory.Lords;
using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory
{
    class StoryVillageMP : Settlement
    {
        private int ticks;
        private int ticksTillReinforcements = -1;
        private bool sacHuntersFleeing;
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
            Scribe_Values.Look(ref mainCharLeftOrDied, "mainCharLeftOrDied");
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, null);
            }, "GeneratingMap", false, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                var storyChar = StoryUtility.GetSWC().questCont.Village.StoryContact;
                //var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, null);
                var label = "StoryVillageArrivedLetterTitle".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID()));
                var text = "StoryVillageArrivedLetterMessage".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())).CapitalizeFirst();

                //var storyContactCell = CellFinder.RandomNotEdgeCell(Math.Min(orGenerateMap.Size.x / 2 - (orGenerateMap.Size.x / 6), orGenerateMap.Size.z / 2 - (orGenerateMap.Size.y / 6)), Map);

                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, Faction, null, null, null);
                CaravanEnterMapUtility.Enter(caravan, Map, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;

                if (!StoryWC.storyFlags["IntroVillage_Entered"])
                {
                    if (!CellFinder.TryFindRandomSpawnCellForPawnNear_NewTmp(new IntVec3(Map.Size.x / 2, 0, Map.Size.z / 2), Map, out var storyContactCell))
                    {
                        Log.Error("Couldn't find a cell to spawn pawn");
                    }
                    // todo handle case if no position was found!!!
                    /*if (storyChar?.Map != orGenerateMap)*/
                    GenSpawn.Spawn(storyChar, storyContactCell, Map);

                    StoryUtility.AssignVillageDialog();
                    AddNewLordAndAssignStoryChar(storyChar);

                    StoryWC.SetSF("IntroVillage_Entered");
                }
            }, "StoryVillageEnterMapMessage", false, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
        }

        private void AddNewLordAndAssignStoryChar(Pawn storyChar)
        {
            centerPoint = StoryUtility.GetCenterOfSettlementBase(Map, StoryUtility.FactionOfSacrilegHunters);
            var raidLords = Map.lordManager.lords.Where(lord => lord.faction == StoryUtility.FactionOfSacrilegHunters);
            Log.Message($"hunter lords: {raidLords.Select(lord => lord.ownedPawns).Count()}");

            foreach (var lord in raidLords.Reverse())
            {
                var pawnsToReassign = lord.ownedPawns;
                lord.lordManager.RemoveLord(lord);
                LordMaker.MakeNewLord(Faction, new LordJob_DefendBaseAgaintHostiles(Faction, centerPoint), Map, pawnsToReassign);
            }

            var selLord = raidLords.OrderByDescending(x => x.ownedPawns.Count).FirstOrDefault();
            if (storyChar.GetLord() != null && storyChar.GetLord() != selLord) storyChar.GetLord().ownedPawns.Remove(storyChar);
            if (storyChar.GetLord() == null) selLord.AddPawn(storyChar);
        }

        public override void PostMapGenerate()
        {

        }

        public void ConversationFinished(Pawn initiator, Pawn addressed)
        {
            Log.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            var endDiaNodeAccepted = new DiaNode("StoryVillage_Dia1_3".Translate());
            endDiaNodeAccepted.options.Add(new DiaOption("StoryVillage_Dia1_3_Option1".Translate()) { action = () => SpawnMechArmy(initiator, addressed), resolveTree = true });

            var subDiaNode = new DiaNode("StoryVillage_Dia1_2".Translate());
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option1".Translate()) { link = endDiaNodeAccepted });
            subDiaNode.options.Add(new DiaOption("StoryVillage_Dia1_2_Option2".Translate()) { link = endDiaNodeAccepted });

            diaNode = new DiaNode("StoryVillage_Dia1_1".Translate(initiator.NameShortColored));
            diaNode.options.Add(new DiaOption("StoryVillage_Dia1_1_Option1".Translate()) { link = subDiaNode }); ;

            TaggedString taggedString = "StoryVillage_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void SpawnMechArmy(Pawn initiator, Pawn addressed)
        {
            StoryWC.SetSF("IntroVillage_TalkedToFriend");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillage_QuestUpdate_MechsArrived".Translate(addressed.NameShortColored));

            var incidentParms = new IncidentParms
            {
                target = Map,
                //incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
                points = 32000,
                faction = Faction.OfMechanoids,
                // todo - find out how to ensure mixed pawngroupmakerkinds
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
            };
            Log.Message($"Default threat points: {StorytellerUtility.DefaultThreatPointsNow(incidentParms.target)}");
            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);

            ticksTillReinforcements = 60 * 15;
            StoryWC.SetSF("IntroVillage_MechsArrived");

            GetComponent<TimedDetectionPatrols>().Init();
            GetComponent<TimedDetectionPatrols>().StartDetectionCountdown(30000, -1);
        }

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.StoryVillageMG;

        public override void Tick()
        {
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
                    StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map, 11000, 12000, centerPoint);
                    StoryWC.SetSF("IntroVillage_ReinforcementsArrived");
                    ReinforcementConvo();
                }

                ticks++;
                ticksTillReinforcements--;
            }

           
        }

        private void CheckMainStoryCharDiedOrLeft()
        {
            if (mainCharLeftOrDied) return;
            var storyChar = StoryUtility.GetSWC().questCont.Village.StoryContact;
            if (!StoryWC.storyFlags["IntroVillage_MechsArrived"]) return;
            if (Map.mapPawns.AllPawnsSpawned.Contains(storyChar) && !storyChar.Dead && !storyChar.Downed) return;

            DiaNode diaNode = null;
            diaNode = new DiaNode(storyChar.Dead ? "StoryVillage_Dia3_1_Dying".Translate() : "StoryVillage_Dia3_1_Alive".Translate());
            diaNode.options.Add(new DiaOption("StoryVillage_Dia3_1_Option1".Translate()) { resolveTree = true }); ;

            TaggedString taggedString = "StoryVillage_Dia3_Title".Translate(storyChar.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
            mainCharLeftOrDied = true;
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

        private void CheckPlayerLeftAndAbandon()
        {
            if (!StoryWC.storyFlags["IntroVillage_MechsArrived"] || !HasMap) return;
            if (Map.mapPawns.FreeColonistsSpawned.Any(x => !x.Dead)) return;

            Log.Message($"Should set player won flag");

            if (!Map.mapPawns.AllPawnsSpawned.Any(x => x.Faction == Faction.OfMechanoids && !x.Dead && !x.Downed))
            {
                Log.Message($"Setting player won flag");
                StoryWC.SetSF("IntroVillage_PlayerWon");
            }
            // todo dialog escaped
            // todo keep and just remove the enter ability in case the player should win the fight?

            Current.Game.DeinitAndRemoveMap(Map);
            
            if (!StoryWC.storyFlags["IntroVillage_PlayerWon"])
            {
                this.Destroy();
                WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
                worldObject.Tile = this.Tile;
                worldObject.SetFaction(this.Faction);
                Find.WorldObjects.Add(worldObject);
            }

            StoryWC.SetSF("IntroVillage_Finished");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "\n\nYou survivded and made it out alive. But where did all those mechs come from? Best to watch out for more clues...");
            Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);
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

            TaggedString taggedString = "StoryVillage_Dia2_Title".Translate(StoryUtility.GetSWC().questCont.Village.StoryContact.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (var baseOptions in base.GetFloatMenuOptions(caravan))
            {
                yield return baseOptions;
            }

            foreach (var storyVillageOption in CaravanArrivalAction_StoryVillageMP.GetFloatMenuOptions(caravan, this))
            {
                yield return storyVillageOption;
            }
            yield break;
        }

    }
}
