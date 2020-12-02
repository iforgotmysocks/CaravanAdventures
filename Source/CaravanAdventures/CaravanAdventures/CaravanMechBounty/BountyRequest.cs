using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    // todo add recruitment option to recruit veteran pawns
    // todo create artifacts in shrines that the player can trade in for points
    class BountyRequest
    {
        private Pawn requestor;
        private Faction faction;
        private DiaNode root;
        private int creditsSpent = 0;

        public BountyRequest(DiaNode result, Pawn negotiator, Faction faction)
        {
            this.root = result;
            this.requestor = negotiator;
            this.faction = faction;
        }

        public DiaOption CreateInitialDiaMenu()
        {
            var hostile = Faction.OfPlayer.HostileTo(faction);
            var error = "CABountyExchangeHostile".Translate(faction.NameColored);
            var node = new DiaNode("CABountyExchangeMain".Translate(CompCache.BountyWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { link = GetDropAssistanceStrengthVariaties(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { link = GetItemOverview(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { link = GetRelationHaggleOverview(node) });
            node.options.Add(new DiaOption("CABountyExchangeRecruitVeteranHunter".Translate()) { link = GetRecuitmentOverview(node), disabled = hostile, disabledReason = error });
            if (Helper.Debug())
            {
                node.options.Add(new DiaOption("Debug: + 1000") { action = () => CompCache.BountyWC.BountyPoints += 1000, link = node });
                node.options.Add(new DiaOption("Debug: - 500") { action = () => CompCache.BountyWC.BountyPoints -= 500, link = node });
            }

            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = root });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = node };
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
            if (CompCache.BountyWC.OngoingEnvoyDelay <= 0) return "CABountyExchangeRequestHelp_StrengthSelection_TroopDeploymentReady".Translate();
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
            Find.WorldTargeter.BeginTargeting_NewTemp(ChoseWorldTarget, true, CompLaunchable.TargeterMouseAttachment, true, delegate
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
            foreach (var item in GenerateItemStock(8))
            {
                if (item == null) continue;
                node.options.Add(new DiaOption("CABountyExchangeRequestItem_ItemDetails".Translate(item.LabelCap,
                    ConvertItemValueToBounty(item)))
                {
                    action = () => PurchaseAndDropItem(item),
                    resolveTree = true,
                    disabled = !CanPurchaseWeapon(item, out var reason),
                    disabledReason = reason
                });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private string GetRestockTimeString()
        {
            if (CompCache.BountyWC.OngoingItemDelay <= 0) return "CABountyExchangeRequestItem_ItemRestockNow".Translate() + "CABountyExchangeRequestItem_ItemRestockDetail".Translate(Math.Round(ModSettings.itemRestockDurationInDays, 1).ToString());
            return "CABountyExchangeRequestItem_ItemRestockDetail".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingItemDelay));
        }

        private List<Thing> GenerateItemStock(int itemCount)
        {
            if (CompCache.BountyWC.CurrentTradeItemStock == null) CompCache.BountyWC.CurrentTradeItemStock = new List<Thing>();
            if (CompCache.BountyWC.OngoingItemDelay > 0) return CompCache.BountyWC.CurrentTradeItemStock;
            CompCache.BountyWC.CurrentTradeItemStock.Clear();
            for (int i = 0; i < itemCount; i++) CompCache.BountyWC.CurrentTradeItemStock.Add(GenerateItem(500 * (i + 1), CompCache.BountyWC.CurrentTradeItemStock));
            CompCache.BountyWC.OngoingItemDelay = ModSettings.itemRestockDurationInDays * 60000;
            return CompCache.BountyWC.CurrentTradeItemStock;
        }

        private Thing GenerateItem(float credits, List<Thing> itemsToAvoid)
        {
            //var rewards = RewardsGenerator.Generate(new RewardsGeneratorParams() { thingRewardItemsOnly = true, minGeneratedRewardValue = credits * 2, disallowedThingDefs = itemsToAvoid.Select(x => x.def).ToList() });
            //var rewardItems = rewards.FirstOrDefault((x) => x is Reward_Items) as Reward_Items;

            var rewardItems = new Reward_Items();
            rewardItems.InitFromValue(credits * 2, new RewardsGeneratorParams() { thingRewardItemsOnly = true, minGeneratedRewardValue = credits * 2, disallowedThingDefs = itemsToAvoid.Select(x => x.def).ToList() }, out var usedCredits);
            return rewardItems.ItemsListForReading.FirstOrDefault();
        }

        private bool CanPurchaseWeapon(Thing thing, out string reason)
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
            // todo not finished! -> come up with valid conversion
            var value = thing.MarketValue * thing.stackCount;
            return value / 2f;
        }

        private void PurchaseAndDropItem(Thing item)
        {
            var things = new List<Thing>() { item };
            if (!things.Any() || item == null)
            {
                Log.Warning($"item was null, skipping");
                return;
            }
            var foundSpot = DropCellFinder.TryFindDropSpotNear(requestor.Position, requestor.Map, out var validPosition, false, false);
            if (!foundSpot)
            {
                Log.Warning($"Not able to drop requested item");
                return;
            }

            if (ModsConfig.RoyaltyActive && item.def == ThingDefOf.PsychicAmplifier) Find.History.lastPsylinkAvailable = Find.TickManager.TicksGame;
            var activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things, true, false);
            DropPodUtility.MakeDropPodAt(validPosition, requestor.Map, activeDropPodInfo);
            Messages.Message("CABountyExchangeRequestItem_ItemArrived".Translate(this.faction.Named("FACTION")), new LookTargets(validPosition, requestor.Map), MessageTypeDefOf.NeutralEvent, true);
            CompCache.BountyWC.BountyPoints -= ConvertItemValueToBounty(item);
            CompCache.BountyWC.CurrentTradeItemStock.Remove(item);
        }

        #endregion

        #region relation haggling
        private DiaNode GetRelationHaggleOverview(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle".Translate(CompCache.BountyWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithYou".Translate(CalculateGoodWillCost(faction, 25), CalcReqGW(faction, 25), faction.NameColored))
            {
                action = () => TradeRelationForBounty(faction, 25),
                resolveTree = true,
                disabled = CompCache.BountyWC.BountyPoints < CalculateGoodWillCost(faction, 25) || faction.GoodwillWith(Faction.OfPlayer) == 100,
                disabledReason = (faction.GoodwillWith(Faction.OfPlayer) == 100)
                    ? "CABountyExchangeRelationHaggle_AlreadyMax".Translate()
                    : "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate()
            });
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithAnotherFaction".Translate()) { link = GetListOfFactions(node, faction, 25) });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private DiaNode GetListOfFactions(DiaNode parent, Faction faction, int goodwill)
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle_Factions".Translate(CompCache.BountyWC.BountyPoints, GetEnvoyTimeString()));
            foreach (var curFaction in Find.FactionManager.AllFactions.Where(f => !f.def.permanentEnemy))
            {
                if (curFaction == Faction.OfPlayer || curFaction == this.faction) continue;
                node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_Factions_ListedFaction".Translate(
                    curFaction.NameColored,
                    curFaction.GoodwillWith(Faction.OfPlayer),
                    CalcReqGW(curFaction, goodwill),
                    CalculateGoodWillCost(curFaction, goodwill)))
                {
                    action = () => TradeRelationForBounty(curFaction, goodwill),
                    resolveTree = true,
                    disabled = !IsHagglingPossible(curFaction, out var reason),
                    disabledReason = reason
                });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

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

        private void TradeRelationForBounty(Faction faction, int goodwill)
        {
            var price = CalculateGoodWillCost(faction, 25);
            var result = faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwill);
            if (!result)
            {
                Log.Warning($"Buying relations of {goodwill} goodwill for faction {faction.Name} failed");
                return;
            }
            CompCache.BountyWC.BountyPoints -= price;
            CompCache.BountyWC.OngoingEnvoyDelay = ModSettings.envoyDurationTimeForBountyRelationHagglingInDays * 60000f;
        }

        private float CalculateGoodWillCost(Faction faction, int goodwill)
        {
            var costPerPoint = 5f;
            if (faction != this.faction) costPerPoint *= 2f;
            var gwbf = faction.GoodwillWith(Faction.OfPlayer);
            if (gwbf < 0) costPerPoint += Math.Abs(gwbf) * 0.1f;
            if (gwbf + goodwill > 100) goodwill -= (gwbf + goodwill - 100);
            return costPerPoint * goodwill;
        }

        #endregion

        #region recruit veteran
        private DiaNode GetRecuitmentOverview(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeVeteranRecruitment".Translate(CompCache.BountyWC.BountyPoints, GetVeteranTimeString()));
            node.options.Add(new DiaOption("CABountyExchangeRequestHelp_StrengthSelection_Few".Translate(5000)) 
            { 
                action = () => EnlistVeteran(), 
                resolveTree = true, 
                disabled = CompCache.BountyWC.BountyPoints < 5000, 
                disabledReason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate()
            });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private void EnlistVeteran()
        {
            var genPawnRequest = new PawnGenerationRequest(CaravanStory.StoryDefOf.CASacrilegHunters_ExperiencedHunter, Faction.OfPlayer)
            {
                AllowGay = false,
                MustBeCapableOfViolence = true,
                ProhibitedTraits = new TraitDef[] { TraitDef.Named("Wimp") },
                ForcedTraits = new TraitDef[] { TraitDef.Named("Tough") },
            };
            var veteran = PawnGenerator.GeneratePawn(genPawnRequest);
            veteran.skills.skills.Where(skill => skill.Named())
        }

        private string GetVeteranTimeString()
        {
            if (CompCache.BountyWC.OngoingEnvoyDelay <= 0) return "CABountyExchangeVeteranRecruitment_Available".Translate();
            return "CABountyExchangeVeteranRecruitment_Details".Translate(CompCache.BountyWC.GetNextAvailableDateInDays(CompCache.BountyWC.OngoingVeteranDelay));
        }
        #endregion


    }
}
