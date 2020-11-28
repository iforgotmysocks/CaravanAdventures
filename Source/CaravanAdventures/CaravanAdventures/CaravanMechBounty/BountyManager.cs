using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyManager
    {
        internal static DiaOption CreateInitialDiaMenu(DiaNode result, Pawn negotiator, Faction faction)
        {
            var bountyNode = new DiaNode("CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints));
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestHelp".Translate()) { action = () => { CompCache.StoryWC.BountyPoints += 100; bountyNode.text = "CABountyExchangeMain".Translate(CompCache.StoryWC.BountyPoints); }, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestItem".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });
            bountyNode.options.Add(new DiaOption("CABountyExchangeRequestImprovedRelations".Translate()) { action = () => CompCache.StoryWC.BountyPoints += 100, link = bountyNode });

            bountyNode.options.Add(new DiaOption("CABountyBack".Translate()) { link = result });

            return new DiaOption("CABountyExchangeOpenOption".Translate()) { link = bountyNode };
        }
    }
}
