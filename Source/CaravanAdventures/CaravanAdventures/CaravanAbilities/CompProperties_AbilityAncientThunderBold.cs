using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class CompProperties_AbilityAncientThunderBold : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityAncientThunderBold()
        {
            this.compClass = typeof(CompAbilityEffect_AncientThunderBold);
        }
    }
}
