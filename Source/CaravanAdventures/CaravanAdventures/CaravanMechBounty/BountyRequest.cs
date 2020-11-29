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
            var node = new DiaNode("CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints));
            node.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { link = GetDropAssistanceStrengthVariaties(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { link = GetItemOverview(node), disabled = hostile, disabledReason = error });
            node.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { link = GetRelationHaggleOverview(node) });
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = root });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = node };
        }

        #region allied military assistance
        private DiaNode GetDropAssistanceStrengthVariaties(DiaNode parent)
        {
            var node = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.StoryWC.BountyPoints));
            //bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { SelectTargetForDrop(requestor); bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, resolveTree = true });
            node.options.Add(HunterDropRequest(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            node.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            node.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        private DiaOption HunterDropRequest(int credit, string label)
        {
            var option = new DiaOption(label.Translate());
            if (CompCache.StoryWC.BountyPoints < credit)
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
            CompCache.StoryWC.BountyPoints -= creditsSpent;
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
            var node = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.StoryWC.BountyPoints));
            //node.options.Add(GenerateItem(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            //node.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            //node.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            //node.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return node;
        }

        #endregion

        #region relation haggling
        private DiaNode GetRelationHaggleOverview(DiaNode bountyNode)
        {
            return new DiaNode("todo");
        }

        #endregion


    }
}
