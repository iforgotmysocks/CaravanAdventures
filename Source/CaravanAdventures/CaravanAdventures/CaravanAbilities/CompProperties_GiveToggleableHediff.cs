using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class CompProperties_GiveToggleableHediff : CompProperties_AbilityGiveHediff
    {
        public CompProperties_GiveToggleableHediff()
        {
            this.compClass = typeof(CompAbilityEffect_GiveToggleableHediff);
        }

        public HediffDef hediffSecondary;
    }
}
