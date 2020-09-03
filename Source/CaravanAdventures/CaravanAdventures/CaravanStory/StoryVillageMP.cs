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

        public StoryVillageMP()
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                var storyChar = StoryUtility.GetSWC().questCont.Village.StoryContact;
                Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, null);
                var label = "StoryVillageArrivedLetterTitle".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID()));
                var text = "StoryVillageArrivedLetterMessage".Translate(Label.ApplyTag(TagType.Settlement, Faction.GetUniqueLoadID())).CapitalizeFirst();

                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, Faction, null, null, null);
                CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, true, null);
                //var storyContactCell = CellFinder.RandomNotEdgeCell(Math.Min(orGenerateMap.Size.x / 2 - (orGenerateMap.Size.x / 6), orGenerateMap.Size.z / 2 - (orGenerateMap.Size.y / 6)), Map);
                CellFinder.TryFindRandomSpawnCellForPawnNear_NewTmp(new IntVec3(orGenerateMap.Size.x / 2, 0, orGenerateMap.Size.z / 2), orGenerateMap, out var storyContactCell);
                // todo handle case if no position was found!!!
                /*if (storyChar?.Map != orGenerateMap)*/ GenSpawn.Spawn(storyChar, storyContactCell, orGenerateMap);
                
                StoryUtility.AssignVillageDialog();
                AddNewLordAndAssignStoryChar(storyChar);

                StoryWC.SetSF("IntroVillage_Entered");
            }, "StoryVillageEnterMapMessage", false, null, true);
        }

        private void AddNewLordAndAssignStoryChar(Pawn storyChar)
        {
            // todo centerpoint is still off -> only calculate rooms that belong to sac hunters
            var centerPoint = StoryUtility.GetCenterOfSettlementBase(Map, StoryUtility.FactionOfSacrilegHunters);
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
        }

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.StoryVillageMG;

        public override void Tick()
        {
            if (base.HasMap)
            {
                if (ticks >= 60)
                {
                    CheckPlayerLeft();
                    CheckShouldSacHuntersFlee();
                    CheckMainStoryCharDiedOrLeft();
                    ticks = 0;
                }
                if (ticksTillReinforcements == 0)
                {
                    // todo do reinforcements
                    var hunters = Map.mapPawns.AllPawnsSpawned.Where(x => x.Faction == StoryUtility.FactionOfSacrilegHunters);
                    StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map, 10000, 11000, hunters?.InRandomOrder()?.FirstOrDefault()?.Position);
                  
                    ReinforcementConvo();
                    SendHuntersToAttack();
                }

                ticks++;
                ticksTillReinforcements--;
            }
        }

        private void CheckMainStoryCharDiedOrLeft()
        {
            var storyChar = StoryUtility.GetSWC().questCont.Village.StoryContact;
            if (!StoryWC.storyFlags["IntroVillage_MechsArrived"]) return;
            if (Map.mapPawns.AllPawnsSpawned.Contains(storyChar)) return;

            // todo StoryChar died Dialog
        }

        private void CheckShouldSacHuntersFlee()
        {
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

            LordMaker.MakeNewLord(Faction, new LordJob_ExitMapBest(LocomotionUrgency.Sprint, false, false), Map, hunters);
        }

        private void CheckPlayerLeft()
        {
            if (!StoryWC.storyFlags["IntroVillage_MechsArrived"]) return;
            if (Map.mapPawns.FreeColonists.Any(x => !x.Dead)) return;

            // todo dialog escaped
            // todo keep and just remove the enter ability in case the player should win the fight?
            this.Destroy();
        }

        private void SendHuntersToAttack()
        {
            var raidLords = Map.lordManager.lords.Where(lord => lord.faction == StoryUtility.FactionOfSacrilegHunters);
            var hunters = new List<Pawn>();
            foreach (var lord in raidLords.Reverse())
            {
                hunters.AddRange(lord.ownedPawns);
                lord.lordManager.RemoveLord(lord);
            }

            hunters.Remove(StoryUtility.GetSWC().questCont.Village.StoryContact);
            //var hunters = Map.mapPawns.AllPawnsSpawned.Where(x => x.Faction == StoryUtility.FactionOfSacrilegHunters && x != StoryUtility.GetSWC().questCont.Village.StoryContact);
            LordMaker.MakeNewLord(Faction, new LordJob_AssaultColony(Faction, false, false, false, false, false), Map, hunters);
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

        protected void CreateMap(Caravan caravan)
        {

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
