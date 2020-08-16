using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class CompProperties_AbilityAncientMechSignal : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityAncientMechSignal()
        {
            this.compClass = typeof(CompAbilityEffect_AncientMechSignal);
        }
    }
}
