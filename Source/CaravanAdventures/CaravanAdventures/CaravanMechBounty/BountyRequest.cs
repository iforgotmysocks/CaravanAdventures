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

        public BountyRequest(DiaNode result, Pawn negotiator, Faction faction)
        {
            this.root = result;
            this.requestor = negotiator;
            this.faction = faction;
        }

        public DiaOption CreateInitialDiaMenu()
        {
            var bountyNode = new DiaNode("CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints));
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { SelectTargetForDrop(requestor); bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, resolveTree = true });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyBack".Translate()) { link = root });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = bountyNode };
        }

        private void SelectTargetForDrop(Pawn negotiator)
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(negotiator.Map.Parent));
            Find.WorldSelector.ClearSelection();
            int tile = negotiator.Map.Tile;
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
            CaravanStory.StoryUtility.GetAssistanceFromAlliedFaction(faction, map, 3000, 3500);
            return true;
        }

        private bool ValidateLaunchTarget(GlobalTargetInfo info)
        {
            return true;
        }
    }
}
