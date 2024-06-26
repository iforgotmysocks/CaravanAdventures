﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyRequest
    {
        private Pawn requestor;
        private Faction faction;
        private DiaNode root;
        private int creditsSpent = 0;
        private TimeSpeed previousTimeSpeed = TimeSpeed.Paused;

        public BountyRequest(DiaNode result, Pawn negotiator, Faction faction)
        {
            this.root = result;
            this.requestor = negotiator;
            this.faction = faction;
        }

        public DiaOption CreateInitialDiaMenu() => new DiaOption("CABountyExchangeOpenOption".Translate()) { linkLateBind = () => CreateMainMenuNode() };

        private DiaNode CreateMainMenuNode()
        {
            var hostile = Faction.OfPlayer.HostileTo(faction);
            var error = "CABountyExchangeHostile".Translate(faction.NameColored);
            var node = new DiaNode("CABountyExchangeMain".Translate(CompCache.BountyWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { link = GetDropAssistanceStrengthVariaties(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { link = GetItemOverview(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { link = GetRelationHaggleOverview() });
            node.options.Add(new DiaOption("CABountyExchangeRecruitVeteranHunter".Translate()) { link = GetRecuitmentOverview(node), disabled = hostile, disabledReason = error });
            if (ModSettings.allowBuyingBountyWithSilver) node.options.Add(new DiaOption("CABountyExchangeBuyBountyWithSilver".Translate()) { link = BuyBountyWithMoneyOverview(node), disabled = hostile, disabledReason = error });
            if (Helper.Debug())
            {
                node.options.Add(new DiaOption("Debug: + 1000") { action = () => CompCache.BountyWC.BountyPoints += 1000, linkLateBind = () => node });
                node.options.Add(new DiaOption("Debug: - 500") { action = () => CompCache.BountyWC.BountyPoints -= 500, linkLateBind = () => node });
            }

            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => root });
            return node;
        }

        #region allied military assistance
        private DiaNode GetDropAssistanceStrengthVariaties(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.BountyWC.BountyPoints, GetTroopsTimeString()));
            node.options.Add(HunterDropRequest(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            node.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            node.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });
            return node;
        }

        private string GetTroopsTimeString()
        {
            if (CompCache.BountyWC.OngoingAlliedAssistanceDelay <= 0) return "CABountyExchangeRequestHelp_StrengthSelection_TroopDeploymentReady".Translate();
            return "CABountyExchangeRequestHelp_StrengthSelection_TroopDeploymentDetail".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingAlliedAssistanceDelay));
        }

        private DiaOption HunterDropRequest(int credit, string label)
        {
            var option = new DiaOption(label.Translate());
            if (CompCache.BountyWC.BountyPoints < credit)
            {
                option.disabled = true;
                option.disabledReason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate();
            }
            else if (CompCache.BountyWC.OngoingAlliedAssistanceDelay > 0)
            {
                option.disabled = true;
                option.disabledReason = "CABountyExchangeRequestHelp_StrengthSelection_TroopDeploymentBusy".Translate();
            }

            if (option.disabled) return option;
            option.resolveTree = true;
            option.action = () =>
            {
                creditsSpent = credit;
                SelectTargetForDrop();
            };
            return option;
        }

        private void SelectTargetForDrop()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(requestor.Map.Parent));
            Find.WorldSelector.ClearSelection();
            int tile = requestor.Map.Tile;
            previousTimeSpeed = Find.TickManager.CurTimeSpeed;
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, true, CompLaunchable.TargeterMouseAttachment, true, delegate
            {
                //GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
            }, (GlobalTargetInfo target) => "Select target tile", ValidateLaunchTarget);
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if (!target.IsValid) return false;
            var map = Find.Maps.FirstOrDefault(t => t.Tile == target.Tile && t.mapPawns.AnyColonistSpawned);
            if (map == null) return false;
            if (creditsSpent == 0)
            {
                Log.Warning("Calculation of assistance points for bounty failed, selecting lowest option");
                creditsSpent = 260;
            }
            if (previousTimeSpeed != TimeSpeed.Paused) Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            CompCache.BountyWC.BountyPoints -= creditsSpent;
            CompCache.BountyWC.OngoingAlliedAssistanceDelay = ModSettings.alliedAssistanceDurationInDays * 60000;
            CaravanStory.StoryUtility.GetAssistanceFromAlliedFaction(faction, map, creditsSpent * 2, creditsSpent * 2);
            return true;
        }

        private bool ValidateLaunchTarget(GlobalTargetInfo info)
        {
            return true;
        }
        #endregion

        #region item trade
        private DiaNode GetItemOverview(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRequestItemTitle".Translate(CompCache.BountyWC.BountyPoints, GetRestockTimeString()));
            foreach (var item in GenerateItemStock(ModSettings.itemStockAmount, 1, ModSettings.buyableGeneAmount))
            {
                if (item == null) continue;
                var link = new Dialog_InfoCard.Hyperlink { thing = item.GetInnerIfMinified(), def = item.GetInnerIfMinified().def };
                var personaTraitString = string.Empty;

                var comp = item.TryGetComp<CompBladelinkWeapon>();
                if (comp != null && ModsConfig.RoyaltyActive)
                {
                    var names = comp.TraitsListForReading.Count != 0 ? comp.TraitsListForReading.Select(trait => trait.LabelCap) : null;
                    if (names != null) personaTraitString = " (" + string.Join(", ", names) + ")";
                }


                if (ModsConfig.BiotechActive && item is Genepack genepack)
                {
                    var effects = genepack.GeneSet?.GenesListForReading?.Select(g => g.LabelCap);
                    if (effects != null) personaTraitString += " (" + string.Join(", ", effects) + ")";
                }

                node.options.Add(new DiaOption((item.LabelCap + personaTraitString))
                {
                    //disabled = !CanPurchaseItem(item, out var reason),
                    disabled = false,
                    disabledReason = string.Empty,
                    hyperlink = link
                });

                node.options.Add(new DiaOption("CABountyExchangeRequestItem_ItemDetails".Translate(item.LabelCap,
                    ConvertItemValueToBounty(item)))
                {
                    action = () => PurchaseAndDropItem(item),
                    resolveTree = false,
                    linkLateBind = () => GetItemOverview(parent),
                    disabled = !CanPurchaseItem(item, out var reason),
                    disabledReason = reason,
                });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => CreateMainMenuNode() });
            return node;
        }

        private IEnumerable<Thing> GenerateItemStock(object itemStockAmount, int v)
        {
            throw new NotImplementedException();
        }

        private string GetRestockTimeString()
        {
            if (CompCache.BountyWC.OngoingItemDelay <= 0) return "CABountyExchangeRequestItem_ItemRestockNow".Translate() + "CABountyExchangeRequestItem_ItemRestockDetail".Translate(Math.Round(ModSettings.itemRestockDurationInDays, 1).ToString());
            return "CABountyExchangeRequestItem_ItemRestockDetail".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingItemDelay));
        }

        private object[] customRewardsRoyalty = !ModsConfig.RoyaltyActive ? new object[] { } : new object[] { ThingCategoryDef.Named("WeaponsMeleeBladelink"), ThingDef.Named("AnimusStone") };
        private List<object> customRewards = new List<object>() { DefDatabase<ThingDef>.GetNamedSilentFail("VanometricPowerCell"), DefDatabase<ThingDef>.GetNamedSilentFail("InfiniteChemreactor") };
        private List<GeneDef> customRewardsGene = new List<GeneDef>() {
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "Ageless"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "TotalHealing"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "DiseaseFree"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "PerfectImmunity"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "Deathless"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "ArchiteMetabolism"),
            DefDatabase<GeneDef>.AllDefs.FirstOrDefault(x => x.defName == "XenogermReimplanter")
        };

        private List<Thing> GenerateItemStock(int itemCount, int customItemCount = 0, int customGeneCount = 0)
        {
            if (ModsConfig.RoyaltyActive && !Helper.ExpRM) customRewards.AddRange(customRewardsRoyalty);
            if (!ModsConfig.BiotechActive || !ModSettings.useGeneRewards || Helper.ExpRM) customGeneCount = 0;
            if (Helper.ExpRM) customItemCount = 0;

            if (CompCache.BountyWC.CurrentTradeItemStock == null) CompCache.BountyWC.CurrentTradeItemStock = new List<Thing>();
            if (CompCache.BountyWC.OngoingItemDelay > 0) return CompCache.BountyWC.CurrentTradeItemStock;
            CompCache.BountyWC.CurrentTradeItemStock.Clear();
            for (int i = 0; i < itemCount; i++) CompCache.BountyWC.CurrentTradeItemStock.Add(GenerateItem(500 * (i + 1), CompCache.BountyWC.CurrentTradeItemStock));
            for (int i = 0; i < customItemCount; i++) CompCache.BountyWC.CurrentTradeItemStock.Add(GenerateItem(0, CompCache.BountyWC.CurrentTradeItemStock, customRewards));
            for (int i = 0; i < customGeneCount; i++) CompCache.BountyWC.CurrentTradeItemStock.Add(GenerateItem(0, CompCache.BountyWC.CurrentTradeItemStock, customRewardsGene));
            CompCache.BountyWC.OngoingItemDelay = ModSettings.itemRestockDurationInDays * 60000;
            return CompCache.BountyWC.CurrentTradeItemStock;
        }

        private Thing GenerateItem(float credits, List<Thing> itemsToAvoid)
        {
            var rewardItems = new Reward_Items();
            rewardItems.InitFromValue(credits * 2, new RewardsGeneratorParams() { thingRewardItemsOnly = true, minGeneratedRewardValue = credits * 2, disallowedThingDefs = itemsToAvoid.Select(x => x.def).ToList() }, out var usedCredits);
            return rewardItems.ItemsListForReading.FirstOrDefault();
        }

        private Thing GenerateItem(float credits, List<Thing> itemsToAvoid, List<object> customItems)
        {
            if (!customItems?.Any() ?? true) return null;
            Thing customReward = null;
            object picked = customItems.RandomElementByWeight(item =>
            {
                if (item is ThingCategoryDef catDef && catDef == ThingCategoryDef.Named("WeaponsMeleeBladelink")) return 0.8f;
                return 0.3f;
            });

            if (picked is ThingDef pickedDef) customReward = ThingMaker.MakeThing(pickedDef);
            else if (picked is ThingCategoryDef pickedCat) customReward = ThingMaker.MakeThing(pickedCat.childThingDefs.RandomElement());
            if (customReward != null && customReward.TryGetQuality(out var quality))
            {
                quality = (QualityCategory)Rand.RangeInclusive(2, 6);
                customReward.TryGetComp<CompQuality>().SetQuality(quality, ArtGenerationContext.Outsider);
            }

            if (customReward is Building) customReward = customReward.TryMakeMinified();

            return customReward;
        }

        private Thing GenerateItem(float credits, List<Thing> itemsToAvoid, List<GeneDef> customItems)
        {
            if (!ModsConfig.BiotechActive || (!customItems?.Any() ?? true)) return null;
            var container = ThingDefOf.Genepack;
            var containerItem = ThingMaker.MakeThing(container) as Genepack;
            var existingGene = containerItem?.GeneSet?.GenesListForReading?.FirstOrDefault();
            if (existingGene == null) return null;
            if (ModSettings.architeGeneChance == 0 || !Rand.Chance(ModSettings.architeGeneChance / 100f)) return containerItem;
            foreach (var gene in containerItem.GeneSet.GenesListForReading.Reverse<GeneDef>()) containerItem.GeneSet.Debug_RemoveGene(gene);
            containerItem.GeneSet.AddGene(customItems.RandomElement());
            return containerItem;
        }

        private bool CanPurchaseItem(Thing thing, out string reason)
        {
            var cost = ConvertItemValueToBounty(thing);
            if (CompCache.BountyWC.BountyPoints < cost)
            {
                reason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate();
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private float ConvertItemValueToBounty(Thing thing)
        {
            var value = thing.MarketValue * thing.stackCount;
            return (float)Math.Round(value * ModSettings.bountyValueMult, 2);
        }

        private void PurchaseAndDropItem(Thing item, int fixedCredits = 0)
        {
            var things = new List<Thing>() { item };
            if (!things.Any() || item == null)
            {
                Log.Warning($"item was null, skipping");
                return;
            }
            var validPosition = DropCellFinder.TradeDropSpot(requestor.Map);
            if (validPosition == default)
            {
                Log.Warning($"Not able to drop requested item at this location");
                Messages.Message($"Not able to drop requested item at this location", MessageTypeDefOf.NegativeEvent);
                return;
            }

            if (ModsConfig.RoyaltyActive && item.def == ThingDefOf.PsychicAmplifier) Find.History.lastPsylinkAvailable = Find.TickManager.TicksGame;
            var activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things, true, false);
            DropPodUtility.MakeDropPodAt(validPosition, requestor.Map, activeDropPodInfo);
            Messages.Message("CABountyExchangeRequestItem_ItemArrived".Translate(this.faction.Named("FACTION")), new LookTargets(validPosition, requestor.Map), MessageTypeDefOf.NeutralEvent, true);
            PayBountyCredits(fixedCredits != 0 ? fixedCredits : ConvertItemValueToBounty(item));
            CompCache.BountyWC.CurrentTradeItemStock.Remove(item);
        }

        private void PayBountyCredits(float cost, bool allowNegativeResult = false)
        {
            var payable = !allowNegativeResult && CompCache.BountyWC.BountyPoints - cost < 0 ? CompCache.BountyWC.BountyPoints : cost;
            CompCache.BountyWC.BountyPoints -= payable;
        }

        #endregion

        #region relation haggling
        private DiaNode GetRelationHaggleOverview()
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle".Translate(CompCache.BountyWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithYou".Translate(CalculateGoodWillCost(faction, 25), CalcReqGW(faction, 25), faction.NameColored))
            {
                action = () => TradeRelationForBounty(faction, 25, false),
                resolveTree = false,
                linkLateBind = () => GetRelationHaggleOverview(),
                disabled = CompCache.BountyWC.BountyPoints < CalculateGoodWillCost(faction, 25) || faction.GoodwillWith(Faction.OfPlayer) == 100,
                disabledReason = (faction.GoodwillWith(Faction.OfPlayer) == 100)
                    ? "CABountyExchangeRelationHaggle_AlreadyMax".Translate()
                    : "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate()
            });
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithAnotherFaction".Translate()) { link = GetListOfFactions(node, faction, 25) });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => CreateMainMenuNode() });
            return node;
        }

        private DiaNode GetListOfFactions(DiaNode parent, Faction faction, int goodwill)
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle_Factions".Translate(CompCache.BountyWC.BountyPoints, GetEnvoyTimeString()));
            foreach (var curFaction in Find.FactionManager.AllFactions.Where(f => !f.def.permanentEnemy && !f.def.hidden && !f.temporary))
            {
                if (curFaction == Faction.OfPlayer || curFaction == this.faction) continue;
                node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_Factions_ListedFaction".Translate(
                    curFaction.NameColored,
                    curFaction.GoodwillWith(Faction.OfPlayer),
                    CalcReqGW(curFaction, goodwill),
                    CalculateGoodWillCost(curFaction, goodwill)))
                {
                    action = () => TradeRelationForBounty(curFaction, goodwill),
                    resolveTree = false,
                    linkLateBind = () => GetListOfFactions(parent, faction, goodwill),
                    disabled = !IsHagglingPossible(curFaction, out var reason),
                    disabledReason = reason
                });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => GetRelationHaggleOverview() });
            return node;
        }

        private string GetEnvoyTimeString()
        {
            if (CompCache.BountyWC.OngoingEnvoyDelay <= 0) return "CABountyExchangeRelationHaggle_EnvoyReady".Translate();
            return "CABountyExchangeRelationHaggle_EnvoyBusyDetail".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingEnvoyDelay));
        }

        private bool IsHagglingPossible(Faction curFaction, out string reason)
        {
            if (CompCache.BountyWC.BountyPoints < CalculateGoodWillCost(curFaction, 25))
            {
                reason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate();
                return false;
            }
            if (CompCache.BountyWC.OngoingEnvoyDelay > 0)
            {
                reason = "CABountyExchangeRelationHaggle_EnvoyBusy".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingEnvoyDelay));
                return false;
            }
            if (curFaction.GoodwillWith(Faction.OfPlayer) == 100)
            {
                reason = "CABountyExchangeRelationHaggle_AlreadyMax".Translate();
                return false;
            }
            if (faction.HostileTo(Faction.OfPlayer))
            {
                reason = "CABountyExchangeHostile".Translate(faction.def.LabelCap);
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private int CalcReqGW(Faction curFaction, int goodwill)
        {
            var curGW = curFaction.GoodwillWith(Faction.OfPlayer);
            if (curGW + goodwill > 100) goodwill -= (curGW + goodwill - 100);
            return goodwill;
        }

        private void TradeRelationForBounty(Faction faction, int goodwill, bool sendEnvoy = true)
        {
            var price = CalculateGoodWillCost(faction, 25);
            var result = faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill);
            if (!result)
            {
                Log.Warning($"Buying relations of {goodwill} goodwill for faction {faction.Name} failed");
                return;
            }
            CompCache.BountyWC.BountyPoints -= price;
            if (sendEnvoy) CompCache.BountyWC.OngoingEnvoyDelay = ModSettings.envoyDurationTimeForBountyRelationHagglingInDays * 60000f;
        }

        private float CalculateGoodWillCost(Faction faction, int goodwill)
        {
            var costPerPoint = 2.5f;
            if (faction != this.faction) costPerPoint *= 2f;
            var gwbf = faction.GoodwillWith(Faction.OfPlayer);
            if (gwbf < 0) costPerPoint += Math.Abs(gwbf) * 0.1f;
            if (gwbf + goodwill > 100) goodwill -= (gwbf + goodwill - 100);
            return (float)Math.Round(costPerPoint * goodwill, 2);
        }

        #endregion

        #region recruit veteran
        private DiaNode GetRecuitmentOverview(DiaNode parent)
        {
            var cost = 4000;
            var costTribal = 6000;
            var node = new DiaNode("CABountyExchangeVeteranRecruitment".Translate(CompCache.BountyWC.BountyPoints, faction.def.LabelCap, GetVeteranTimeString()));
            node.options.Add(new DiaOption("CABountyExchangeVeteranRecruitment_Enlist".Translate(cost))
            {
                link = PickVeteranPersonality(node, cost),
                disabled = !CanRecruitVeteran(cost, out var reason),
                disabledReason = reason
            });
            if (ModsConfig.RoyaltyActive)
            {
                node.options.Add(new DiaOption("CABountyExchangeVeteranRecruitment_EnlistTribal".Translate(costTribal))
                {
                    link = PickVeteranPersonality(node, costTribal, true),
                    disabled = !CanRecruitVeteran(costTribal, out reason),
                    disabledReason = reason
                });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => CreateMainMenuNode() });
            return node;
        }

        private DiaNode PickVeteranPersonality(DiaNode parent, int cost, bool tribal = false)
        {
            var personalityTraits = new[] {
                 DefDatabase<TraitDef>.GetNamedSilentFail("Cannibal"),
                TraitDefOf.Bloodlust,
                TraitDefOf.Psychopath,
                TraitDefOf.Transhumanist,
                DefDatabase<TraitDef>.GetNamedSilentFail("Nerves"),
                DefDatabase<TraitDef>.GetNamedSilentFail("NaturalMood"),
                DefDatabase<TraitDef>.GetNamedSilentFail("Masochist")};

            var node = new DiaNode("CABountyExchangeVeteranRecruitment_ChoosePersonality".Translate());
            foreach (var personality in personalityTraits)
            {
                if (personality == null) continue;
                node.options.Add(new DiaOption(personality.degreeDatas.OrderByDescending(data => data.degree).FirstOrDefault().LabelCap)
                {
                    link = PickVeteranSkill(node, cost, personality, tribal),
                });
            }

            node.options.Add(new DiaOption("CABountyExchangeVeteranRecruitment_ChoosePersonalityNone".Translate())
            {
                link = PickVeteranSkill(node, cost, null, tribal),
            });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });
            return node;
        }

        private DiaNode PickVeteranSkill(DiaNode parent, int cost, TraitDef personality, bool tribal = false)
        {
            var skillTraits = new[] {
                DefDatabase<TraitDef>.GetNamedSilentFail("Nimble"),
                DefDatabase<TraitDef>.GetNamedSilentFail("SpeedOffset"),
            };
            var node = new DiaNode("CABountyExchangeVeteranRecruitment_ChooseSkill".Translate());
            foreach (var skill in skillTraits)
            {
                if (skill == null) continue;
                node.options.Add(new DiaOption(skill.degreeDatas.OrderByDescending(data => data.degree).FirstOrDefault().LabelCap)
                {
                    action = () => EnlistVeteran(cost, personality, skill, null, tribal),
                    resolveTree = true,
                });
            }

            node.options.Add(new DiaOption("CABountyExchangeVeteranRecruitment_ChoosePersonalityNone".Translate())
            {
                action = () => EnlistVeteran(cost, personality, null, null, tribal),
                resolveTree = true,
            });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });
            return node;
        }

        private bool CanRecruitVeteran(int cost, out string reason)
        {
            if (ModSettings.debug)
            {
                reason = string.Empty;
                return true;
            }
            if (CompCache.BountyWC.BountyPoints < cost)
            {
                reason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate();
                return false;
            }
            if (CompCache.BountyWC.OngoingVeteranDelay > 0)
            {
                reason = "CABountyExchangeVeteranRecruitment_NotAvailable".Translate();
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void EnlistVeteran(int cost, TraitDef personality, TraitDef skill, Pawn existingVet = null, bool tribal = false)
        {
            Pawn vet = existingVet ?? BountyUtility.GenerateVeteran(personality, skill, tribal);
            if (vet == null)
            {
                Log.Error($"Creating veteran failed, generated and returned pawn was null");
                return;
            }
            PurchaseAndDropItem(vet, cost);
            CompCache.BountyWC.OngoingVeteranDelay = ModSettings.veteranResetTimeInDays * 60000;
        }

        private string GetVeteranTimeString()
        {
            if (CompCache.BountyWC.OngoingVeteranDelay <= 0) return "CABountyExchangeVeteranRecruitment_Available".Translate();
            return "CABountyExchangeVeteranRecruitment_Details".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingVeteranDelay));
        }
        #endregion

        #region buy bounty with silver
        private DiaNode BuyBountyWithMoneyOverview(DiaNode parent)
        {
            var currentSilver = GetCurrentSilver();
            var node = new DiaNode("CABountyExchangeSilverForBounty".Translate(CompCache.BountyWC.BountyPoints, currentSilver, faction.def.LabelCap, GetVeteranTimeString()));
            var cost = 500;
            node.options.Add(new DiaOption("CABountyExchangeSilverForBountyOption".Translate(cost, Convert.ToInt32(cost * ModSettings.bountyValueMult * (1 - ModSettings.bountyCreditPurchaseCostMult))))
            {
                linkLateBind = () => BuyBountyWithMoneyOverview(parent),
                action = () => ExchangeSilverForBounty(cost),
                disabled = !CanAfford(cost, out var reason),
                disabledReason = reason
            });
            var cost2 = 1000;
            node.options.Add(new DiaOption("CABountyExchangeSilverForBountyOption".Translate(cost2, Convert.ToInt32(cost2 * ModSettings.bountyValueMult * (1 - ModSettings.bountyCreditPurchaseCostMult))))
            {
                linkLateBind = () => BuyBountyWithMoneyOverview(parent),
                action = () => ExchangeSilverForBounty(cost2),
                disabled = !CanAfford(cost2, out reason),
                disabledReason = reason
            });
            var cost3 = 5000;
            node.options.Add(new DiaOption("CABountyExchangeSilverForBountyOption".Translate(cost3, Convert.ToInt32(cost3 * ModSettings.bountyValueMult * (1 - ModSettings.bountyCreditPurchaseCostMult))))
            {
                linkLateBind = () => BuyBountyWithMoneyOverview(parent),
                action = () => ExchangeSilverForBounty(cost3),
                disabled = !CanAfford(cost3, out reason),
                disabledReason = reason
            });
            var cost4 = 25000;
            node.options.Add(new DiaOption("CABountyExchangeSilverForBountyOption".Translate(cost4, Convert.ToInt32(cost4 * ModSettings.bountyValueMult * (1 - ModSettings.bountyCreditPurchaseCostMult))))
            {
                linkLateBind = () => BuyBountyWithMoneyOverview(parent),
                action = () => ExchangeSilverForBounty(cost4),
                disabled = !CanAfford(cost4, out reason),
                disabledReason = reason
            });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { linkLateBind = () => CreateMainMenuNode() });
            return node;
        }

        private int GetCurrentSilver() => TradeUtility.AllLaunchableThingsForTrade(requestor.Map)
            .Where(x => x.def == ThingDefOf.Silver)
            .Sum(x => x.stackCount);

        private bool CanAfford(int cost, out string reason)
        {
            var buyable = TradeUtility.ColonyHasEnoughSilver(requestor.Map, cost);
            reason = buyable ? new TaggedString() : "CABountyExchangeSilverForBountyLackingSilver".Translate();
            return buyable;
        }

        private void ExchangeSilverForBounty(int cost)
        {
            if (requestor?.Map?.Biome?.defName == Patches.Compatibility.SoS2Patch.OuterSpaceBiomeName) LaunchSilverFromSpace(ThingDefOf.Silver, cost, requestor.Map, null);
            else TradeUtility.LaunchSilver(requestor.Map, cost);
            CompCache.BountyWC.BountyPoints += Convert.ToInt32(cost * ModSettings.bountyValueMult * (1 - ModSettings.bountyCreditPurchaseCostMult));
        }

        private void LaunchSilverFromSpace(ThingDef resDef, int debt, Map map, TradeShip trader = null)
        {
            while (debt > 0)
            {
                var thing = map.zoneManager.AllZones
                    .SelectMany(x => x.cells)
                    .SelectMany(x => map.thingGrid.ThingsAt(x))
                    .FirstOrDefault(x => x?.def == resDef);

                if (thing == null) thing = map.listerBuildings.AllBuildingsColonistOfClass<Building_Storage>()
                        .SelectMany(x => x.AllSlotCellsList())
                        .SelectMany(x => map.thingGrid.ThingsAt(x))
                        .FirstOrDefault(x => x.def == resDef);

                if (thing == null)
                {
                    Log.Error("Could not find any " + resDef + " to transfer to trader.");
                    return;
                }
                int num = Math.Min(debt, thing.stackCount);
                if (trader != null)
                {
                    trader.GiveSoldThingToTrader(thing, num, TradeSession.playerNegotiator);
                }
                else
                {
                    thing.SplitOff(num).Destroy(DestroyMode.Vanish);
                }
                debt -= num;
            }
        }
        #endregion
    }
}
