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
            var bountyNode = new DiaNode("CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints));
            //bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { SelectTargetForDrop(requestor); bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, resolveTree = true });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { link = GetDropAssistanceStrengthVariaties(bountyNode) });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyBack".Translate()) { link = root });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = bountyNode };
        }

        private DiaNode GetDropAssistanceStrengthVariaties(DiaNode parent)
        {
            var bountyNode = new DiaNode("CABountyExchangeRequestHelp_StrengthSelection".Translate(CompCache.StoryWC.BountyPoints));
            //bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { SelectTargetForDrop(requestor); bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, resolveTree = true });
            bountyNode.options.Add(HunterDropRequest(250, "CABountyExchangeRequestHelp_StrengthSelection_Few"));
            bountyNode.options.Add(HunterDropRequest(650, "CABountyExchangeRequestHelp_StrengthSelection_Bunch"));
            bountyNode.options.Add(HunterDropRequest(1250, "CABountyExchangeRequestHelp_StrengthSelection_Army"));
            bountyNode.options.Add(new DiaOption("CABountyBack".Translate()) { link = parent });

            return bountyNode;
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
    }
}
