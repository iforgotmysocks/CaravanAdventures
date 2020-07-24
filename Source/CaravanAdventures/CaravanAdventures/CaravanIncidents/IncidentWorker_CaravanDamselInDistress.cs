using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;

// todo update broke quest... check out what happend.
// todo cleanup, should prolly remove pawns when leaving early or not killing everyone!

namespace CaravanAdventures.CaravanIncidents
{
    public class IncidentWorker_CaravanDamselInDistress : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction faction;
            return CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile) && PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out faction, null, false, false, false, true);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            parms.points *= IncidentWorker_CaravanDamselInDistress.IncidentPointsFactorRange.RandomInRange;
            Caravan caravan = (Caravan)parms.target;
            if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out parms.faction, null, false, false, false, true))
            {
                return false;
            }

            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, false);
            defaultPawnGroupMakerParms.generateFightersOnly = true;
            defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
            defaultPawnGroupMakerParms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamed("SiegeMechanoid");
            List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
            var girl = DamselInDistressUtility.GenerateGirl(caravan.Tile);

            if (attackers.Count == 0)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Caravan demand incident couldn't generate any enemies even though min points have been checked. faction=",
                    defaultPawnGroupMakerParms.faction,
                    "(",
                    (defaultPawnGroupMakerParms.faction != null) ? defaultPawnGroupMakerParms.faction.def.ToString() : "null",
                    ") parms=",
                    parms
                }), false);
                return false;
            }
            CameraJumper.TryJumpAndSelect(caravan);

            var joinDiaNode = new DiaNode("CaravanDamselInDistress_AsksToJoin".Translate());
            joinDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_JoinAllow".Translate()) { action = () => DamselInDistressUtility.GirlJoins(caravan.pawns.InnerListForReading, girl, caravan), resolveTree = true });
            joinDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_JoinDeny".Translate()) { resolveTree = true });

            var subSubDiaNode = new DiaNode("CaravanDamselInDistress_Follow_KillSuccess_Main".Translate());
            subSubDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow_KillSuccess_Free".Translate()) { link = joinDiaNode });
            subSubDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow_KilSuccess_Slave".Translate()) { action = () => ActionRewardPrisoner(caravan, girl), resolveTree = true });

            var subSubBadDiaNode = new DiaNode("CaravanDamselInDistress_Follow_KillFail_Main".Translate());
            subSubBadDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow_KillFail_Fight".Translate()) { action = () => ActionFight(caravan, attackers, girl), resolveTree = true });
            subSubBadDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow_KillFail_Leave".Translate()) { action = () => ActionLeave(caravan, attackers, girl), resolveTree = true });

            var subDiaNode = new DiaNode("CaravanDamselInDistress_Follow_Main".Translate());
            subDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow_SneakFreeGirl".Translate()) { link = ChanceByHuntingSkill(caravan, subSubDiaNode, subSubBadDiaNode) });
            subDiaNode.options.Add(new DiaOption("CaravanDamselInDistress_Leave".Translate()) { action = () => ActionLeave(caravan, attackers, girl), resolveTree = true });
            
            var diaNode = new DiaNode(this.GenerateMessageText(parms.faction, attackers.Count, caravan));
            diaNode.options.Add(new DiaOption("CaravanDamselInDistress_Rescue".Translate()) { action = () => ActionFight(caravan, attackers, girl), resolveTree = true });
            diaNode.options.Add(new DiaOption("CaravanDamselInDistress_Follow".Translate()) { link = subDiaNode });
            diaNode.options.Add(new DiaOption("CaravanDamselInDistress_Leave".Translate()) { action = () => ActionLeave(caravan, attackers, girl), resolveTree = true });

            TaggedString taggedString = "CaravanDamselInDistressTitle".Translate(parms.faction.Name);
            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, parms.faction, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString, parms.faction));

            return true;
        }

        private void ActionRewardPrisoner(Caravan caravan, Pawn girl)
        {
            girl.guest.SetGuestStatus(caravan.pawns.InnerListForReading.FirstOrDefault()?.Faction ?? Faction.OfPlayer, true);
        }

        private DiaNode ChanceByHuntingSkill(Caravan caravan, DiaNode subSubDiaNode, DiaNode subSubBadDiaNode)
        {
            //var huntingSkill = caravan.pawns.InnerListForReading.Where(x => x.RaceProps.Humanlike).Select(x => x.skills.skills.FirstOrDefault(skill => skill.def == skilldef).Level).Average();
            var huntingSkill = caravan.pawns.InnerListForReading.Where(x => x.RaceProps.Humanlike).Select(x => x.GetStatValue(StatDefOf.HuntingStealth)).Average();
            var chance = Rand.Chance(huntingSkill);
            if (Rand.Chance(huntingSkill)) return subSubDiaNode;
            else return subSubBadDiaNode;
        }

        private void ActionReward(Caravan caravan, List<Pawn> attackers)
        {
            var amount = Rand.Range(200, 500);
            for (int i = 0; i < amount; i++)
            {
                var silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                CaravanInventoryUtility.GiveThing(caravan, silver);
            }
        }

        private void ActionLeave(Caravan caravan, List<Pawn> attackers, Pawn girl)
        {
            for (int i = 0; i < attackers.Count; i++)
            {
                Find.WorldPawns.PassToWorld(attackers[i], PawnDiscardDecideMode.Decide);
            }
            Find.WorldPawns.PassToWorld(girl, PawnDiscardDecideMode.Decide);
        }

        private void ActionFight(Caravan caravan, List<Pawn> attackers, Pawn girl)
        {
            Faction enemyFaction = attackers[0].Faction;
            var damselMapParent = DamselInDistressUtility.NewDamselInDistressMapParent(caravan.Tile, attackers, girl);
            TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, new object[]
            {
                caravan.RandomOwner()
            });
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = DamselInDistressUtility.SetupCaravanAttackMap(damselMapParent, caravan, attackers, true);
                
                LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, true, false, false, false, true);
                if (lordJob_AssaultColony != null)
                {
                    LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
                }
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                CameraJumper.TryJump(attackers[0]);

            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        private string GenerateMessageText(Faction enemyFaction, int attackerCount, Caravan caravan)
        {
            return "CaravanDamselInDistress".Translate(caravan.Name, enemyFaction.Name, attackerCount, "", enemyFaction.def.pawnsPlural).CapitalizeFirst();
        }

        private void TakeFromCaravan(Caravan caravan, List<ThingCount> demands, Faction enemyFaction)
        {
            List<Thing> list = new List<Thing>();
            for (int i = 0; i < demands.Count; i++)
            {
                ThingCount thingCount = demands[i];
                if (thingCount.Thing is Pawn)
                {
                    Pawn pawn = (Pawn)thingCount.Thing;
                    caravan.RemovePawn(pawn);
                    foreach (Thing thing in ThingOwnerUtility.GetAllThingsRecursively(pawn, false))
                    {
                        list.Add(thing);
                        thing.holdingOwner.Take(thing);
                    }
                    if (pawn.RaceProps.Humanlike)
                    {
                        enemyFaction.kidnapped.Kidnap(pawn, null);
                    }
                    else if (!Find.WorldPawns.Contains(pawn))
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
                    }
                }
                else
                {
                    thingCount.Thing.SplitOff(thingCount.Count).Destroy(DestroyMode.Vanish);
                }
            }
            for (int j = 0; j < list.Count; j++)
            {
                if (!list[j].Destroyed)
                {
                    CaravanInventoryUtility.GiveThing(caravan, list[j]);
                }
            }
        }

        private static readonly FloatRange IncidentPointsFactorRange = new FloatRange(1f, 1.7f);

    }
}
