﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;

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
            List<ThingCount> demands = this.GenerateDemands(caravan);
            if (demands.NullOrEmpty<ThingCount>())
            {
                return false;
            }
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, false);
            defaultPawnGroupMakerParms.generateFightersOnly = true;
            defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
            List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
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
            DiaNode diaNode = new DiaNode(this.GenerateMessageText(parms.faction, attackers.Count, demands, caravan));
            DiaOption diaOption = new DiaOption("CaravanDamselInDistress_Rescue".Translate());
            diaOption.action = delegate ()
            {
                this.ActionFight(caravan, attackers);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            diaOption = new DiaOption("CaravanDamselInDistress_Follow".Translate());
            diaOption.action = delegate ()
            {
                this.ActionDialogFollow(caravan, attackers, parms);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            DiaOption diaOption2 = new DiaOption("CaravanDamselInDistress_Leave".Translate());
            diaOption2.action = delegate ()
            {
                this.ActionLeave(caravan, attackers);
            };
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);
            TaggedString taggedString = "CaravanDamselInDistressTitle".Translate(parms.faction.Name);
            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, parms.faction, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString, parms.faction));
            return true;
        }

        private void ActionDialogFollow(Caravan caravan, List<Pawn> attackers, IncidentParms parms)
        {
            var diaNode = new DiaNode("CaravanDamselInDistress_Follow_Main".Translate());
            var option = new DiaOption("CaravanDamselInDistress_Follow_KillAndFreeGirl".Translate());
            option.action = () => KillAndFreeGirl(caravan, attackers, parms);
            diaNode.options.Add(option);

            option = new DiaOption("CaravanDamselInDistress_Follow_SneakFreeGirl".Translate());
            option.action = () => KillAndFreeGirl(caravan, attackers, parms);
            diaNode.options.Add(option);

            option = new DiaOption("CaravanDamselInDistress_Leave".Translate());
            option.action = () => ActionLeave(caravan, attackers);
            diaNode.options.Add(option);

            TaggedString taggedString = "CaravanDamselInDistress_Follow_Title".Translate(parms.faction.Name);

            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, parms.faction, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString, parms.faction));
        }

        private void KillAndFreeGirl(Caravan caravan, List<Pawn> attackers, IncidentParms parms)
        {
            // todo chance based on skill!

            var diaNode = new DiaNode("CaravanDamselInDistress_Follow_KillSuccess_Main".Translate());
            var option = new DiaOption("CaravanDamselInDistress_Follow_KillSuccess_Free".Translate());
            //option.action = () => KillAndFreeGirl(caravan, attackers);
            diaNode.options.Add(option);

            option = new DiaOption("CaravanDamselInDistress_Follow_KilSuccess_Slave".Translate());
            //option.action = () => KillAndFreeGirl(caravan, attackers);
            diaNode.options.Add(option);

            TaggedString taggedString = "CaravanDamselInDistress_Follow_Title".Translate(parms.faction.Name);

            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, parms.faction, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString, parms.faction));
        }

        private List<ThingCount> GenerateDemands(Caravan caravan)
        {
            float num = 1.8f;
            float num2 = Rand.Value * num;
            if (num2 < 0.15f)
            {
                List<ThingCount> list = this.TryGenerateColonistOrPrisonerDemand(caravan);
                if (!list.NullOrEmpty<ThingCount>())
                {
                    return list;
                }
            }
            if (num2 < 0.3f)
            {
                List<ThingCount> list2 = this.TryGenerateAnimalsDemand(caravan);
                if (!list2.NullOrEmpty<ThingCount>())
                {
                    return list2;
                }
            }
            List<ThingCount> list3 = this.TryGenerateItemsDemand(caravan);
            if (!list3.NullOrEmpty<ThingCount>())
            {
                return list3;
            }
            if (Rand.Bool)
            {
                List<ThingCount> list4 = this.TryGenerateColonistOrPrisonerDemand(caravan);
                if (!list4.NullOrEmpty<ThingCount>())
                {
                    return list4;
                }
                List<ThingCount> list5 = this.TryGenerateAnimalsDemand(caravan);
                if (!list5.NullOrEmpty<ThingCount>())
                {
                    return list5;
                }
            }
            else
            {
                List<ThingCount> list6 = this.TryGenerateAnimalsDemand(caravan);
                if (!list6.NullOrEmpty<ThingCount>())
                {
                    return list6;
                }
                List<ThingCount> list7 = this.TryGenerateColonistOrPrisonerDemand(caravan);
                if (!list7.NullOrEmpty<ThingCount>())
                {
                    return list7;
                }
            }
            return null;
        }

        private List<ThingCount> TryGenerateColonistOrPrisonerDemand(Caravan caravan)
        {
            List<Pawn> list = new List<Pawn>();
            int num = 0;
            for (int i = 0; i < caravan.pawns.Count; i++)
            {
                if (caravan.IsOwner(caravan.pawns[i]))
                {
                    num++;
                }
            }
            if (num >= 2)
            {
                for (int j = 0; j < caravan.pawns.Count; j++)
                {
                    if (caravan.IsOwner(caravan.pawns[j]))
                    {
                        list.Add(caravan.pawns[j]);
                    }
                }
            }
            for (int k = 0; k < caravan.pawns.Count; k++)
            {
                if (caravan.pawns[k].IsPrisoner)
                {
                    list.Add(caravan.pawns[k]);
                }
            }
            if (list.Any<Pawn>())
            {
                return new List<ThingCount>
                {
                    new ThingCount(list.RandomElement<Pawn>(), 1)
                };
            }
            return null;
        }

        private List<ThingCount> TryGenerateAnimalsDemand(Caravan caravan)
        {
            int num = 0;
            for (int i = 0; i < caravan.pawns.Count; i++)
            {
                if (caravan.pawns[i].RaceProps.Animal)
                {
                    num++;
                }
            }
            if (num == 0)
            {
                return null;
            }
            int count = Rand.RangeInclusive(1, (int)Mathf.Max((float)num * 0.6f, 1f));
            return (from x in (from x in caravan.pawns.InnerListForReading
                               where x.RaceProps.Animal
                               orderby x.MarketValue descending
                               select x).Take(count)
                    select new ThingCount(x, 1)).ToList<ThingCount>();
        }

        private List<ThingCount> TryGenerateItemsDemand(Caravan caravan)
        {
            List<ThingCount> list = new List<ThingCount>();
            List<Thing> list2 = new List<Thing>();
            list2.AddRange(caravan.PawnsListForReading.SelectMany((Pawn x) => ThingOwnerUtility.GetAllThingsRecursively(x, false)));
            list2.RemoveAll((Thing x) => x.MarketValue * (float)x.stackCount < 50f);
            list2.RemoveAll((Thing x) => x.ParentHolder is Pawn_ApparelTracker && x.MarketValue < 500f);
            float num = list2.Sum((Thing x) => x.MarketValue * (float)x.stackCount);
            float requestedCaravanValue = Mathf.Clamp(IncidentWorker_CaravanDamselInDistress.DemandAsPercentageOfCaravan.RandomInRange * num, 300f, 3500f);
            Func<Thing, bool> somefunc = null;
            while (requestedCaravanValue > 50f)
            {
                IEnumerable<Thing> source = list2;
                Func<Thing, bool> predicate;
                if ((predicate = somefunc) == null)
				{
                    predicate = (somefunc = ((Thing x) => x.MarketValue * (float)x.stackCount <= requestedCaravanValue * 2f));
                }
                Thing thing;
                if (!source.Where(predicate).TryRandomElementByWeight((Thing x) => Mathf.Pow(x.MarketValue / x.GetStatValue(StatDefOf.Mass, true), 2f), out thing))
                {
                    return null;
                }
                int num2 = Mathf.Clamp((int)(requestedCaravanValue / thing.MarketValue), 1, thing.stackCount);
                requestedCaravanValue -= thing.MarketValue * (float)num2;
                list.Add(new ThingCount(thing, num2));
                list2.Remove(thing);
            }
            return list;
        }

        private string GenerateMessageText(Faction enemyFaction, int attackerCount, List<ThingCount> demands, Caravan caravan)
        {
            return "CaravanDamselInDistress".Translate(caravan.Name, enemyFaction.Name, attackerCount, GenLabel.ThingsLabel(demands, "  - ", false), enemyFaction.def.pawnsPlural).CapitalizeFirst();
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

        private void ActionLeave(Caravan caravan, List<Pawn> attackers)
        {
            //this.TakeFromCaravan(caravan, demands, attackers[0].Faction);
            for (int i = 0; i < attackers.Count; i++)
            {
                Find.WorldPawns.PassToWorld(attackers[i], PawnDiscardDecideMode.Decide);
            }
        }

        private void ActionFight(Caravan caravan, List<Pawn> attackers)
        {
            Faction enemyFaction = attackers[0].Faction;
            TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, new object[]
            {
                caravan.RandomOwner()
            });
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, attackers, true);
                LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, true, false, false, false, true);
                if (lordJob_AssaultColony != null)
                {
                    LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
                }
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                CameraJumper.TryJump(attackers[0]);
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        private static readonly FloatRange DemandAsPercentageOfCaravan = new FloatRange(0.05f, 0.2f);

        private static readonly FloatRange IncidentPointsFactorRange = new FloatRange(1f, 1.7f);

        private const float DemandAnimalsWeight = 0.15f;

        private const float DemandColonistOrPrisonerWeight = 0.15f;

        private const float DemandItemsWeight = 1.5f;

        private const float MaxDemandedAnimalsPct = 0.6f;

        private const float MinDemandedMarketValue = 300f;

        private const float MaxDemandedMarketValue = 3500f;

        private const float TrashMarketValueThreshold = 50f;

        private const float IgnoreApparelMarketValueThreshold = 500f;
    }
}
