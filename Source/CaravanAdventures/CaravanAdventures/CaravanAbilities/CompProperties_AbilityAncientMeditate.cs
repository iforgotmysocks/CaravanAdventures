using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class CompProperties_AbilityAncientMeditate : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityAncientMeditate()
        {
            this.compClass = typeof(CompAbilityEffect_AncientMeditate);
        }
    }
}
