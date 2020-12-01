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
    // todo relations:
    // --> add envoy timer -> extract fail reason and disable check into methods that check for money and envoy timer

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
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = root });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = node };
        }

        #region allied military assistance
        private DiaNode GetDropAssistanceStrengthVariaties(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.BountyWC.BountyPoints));
            //bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { SelectTargetForDrop(requestor); bountyNode.text = "CABountyExchangeMain".Translate(CompCache.BountyWC.BountyPoints); }, resolveTree = true });
            node.options.Add(HunterDropRequest(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            node.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            node.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private DiaOption HunterDropRequest(int credit, string label)
        {
            var option = new DiaOption(label.Translate());
            if (CompCache.BountyWC.BountyPoints < credit)
            {
                option.disabled = true;
                option.disabledReason = "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate();
            }

            if (option.disabled) return option;
            option.resolveTree = true;
            option.action = () => {

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
            CaravanStory.StoryUtility.GetAssistanceFromAlliedFaction(faction, map, creditsSpent * 2, creditsSpent * 2);
            return true;
        }

        private bool ValidateLaunchTarget(GlobalTargetInfo info)
        {
            return true;
        }
        #endregion

        #region ancient item trade
        private DiaNode GetItemOverview(DiaNode bountyNode)
        {
            var node = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.BountyWC.BountyPoints));
            //node.options.Add(GenerateItem(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            //node.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            //node.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            //node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        #endregion

        #region relation haggling
        private DiaNode GetRelationHaggleOverview(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle".Translate(CompCache.BountyWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithYou".Translate(CalculateGoodWillCost(faction, 25), CalcReqGW(faction, 25), faction.NameColored)) { action = () => TradeRelationForBounty(faction, 25), disabled = CompCache.BountyWC.BountyPoints < CalculateGoodWillCost(faction, 25) || faction.GoodwillWith(Faction.OfPlayer) == 100, disabledReason = (faction.GoodwillWith(Faction.OfPlayer) == 100) ? "CABountyExchangeRequestHelp_StrengthSelection_AlreadyMax".Translate() : "CABountyExchangeRequestHelp_StrengthSelection_NotEnoughMoney".Translate() });
            node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_WithAnotherFaction".Translate()) { link = GetListOfFactions(node, faction, 25) });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private DiaNode GetListOfFactions(DiaNode parent, Faction faction, int v)
        {
            var node = new DiaNode("CABountyExchangeRelationHaggle_Factions".Translate(CompCache.BountyWC.BountyPoints));
            foreach (var curFaction in Find.FactionManager.AllFactions.Where(f => !f.def.permanentEnemy))
            {
                if (curFaction == Faction.OfPlayer || curFaction == this.faction) continue;
                node.options.Add(new DiaOption("CABountyExchangeRelationHaggle_Factions_ListedFaction".Translate(curFaction.NameColored, curFaction.GoodwillWith(Faction.OfPlayer), CalcReqGW(curFaction, 25), CalculateGoodWillCost(curFaction, 25))) { action = () => TradeRelationForBounty(curFaction, 25), disabled = !IsHagglingPossible(curFaction, out var reason), disabledReason = reason });
            }
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
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
                reason = "CABountyExchangeRequestHelp_StrengthSelection_EnvoyBusy".Translate();
                return false;
            }
            if (curFaction.GoodwillWith(Faction.OfPlayer) == 100)
            {
                reason = "CABountyExchangeRequestHelp_StrengthSelection_AlreadyMax".Translate();
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


    }
}
