using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    class CompAbilityEffect_AncientMeditate : CompAbilityEffect
    {
        //public new CompProperties_AbilityBlackMagicMeditate Props => (CompProperties_AbilityBlackMagicMeditate)this.props;

        public new CompProperties_AbilityGiveHediff Props => (CompProperties_AbilityGiveHediff)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.HasThing)
            {
                base.Apply(target, dest);
                Pawn pawn = target.Thing as Pawn;
                if (pawn != null)
                {
                    var hediff = HediffMaker.MakeHediff(this.Props.hediffDef, this.parent.pawn);
                    pawn.health.AddHediff(hediff, pawn.health.hediffSet.GetBrain());
                }
            }
        }

        
    }
}
