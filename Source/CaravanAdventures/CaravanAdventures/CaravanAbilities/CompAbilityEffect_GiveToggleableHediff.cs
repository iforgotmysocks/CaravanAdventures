using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class CompAbilityEffect_GiveToggleableHediff : CompAbilityEffect
    {
        public new CompProperties_GiveToggleableHediff Props => (CompProperties_GiveToggleableHediff)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (this.Props.onlyApplyToSelf)
            {
                this.ApplyInner(this.parent.pawn, null);
                return;
            }
            this.ApplyInner(target.Pawn, this.parent.pawn);
            if (this.Props.applyToSelf)
            {
                this.ApplyInner(this.parent.pawn, target.Pawn);
            }
        }

        protected void ApplyInner(Pawn target, Pawn other)
        {
            if (target != null)
            {
                var isGifted = target.health.hediffSet.hediffs.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientGift) != null;
                var selectedHediffDef = isGifted ? this.Props.hediffDef : this.Props.hediffSecondary; 
                var firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(selectedHediffDef, false);
                
                if (firstHediffOfDef != null)
                {
                    target.health.RemoveHediff(firstHediffOfDef);
                }
                else
                {
                    var hediff = HediffMaker.MakeHediff(selectedHediffDef, target);
                    target.health.AddHediff(hediff, target.health.hediffSet.GetBrain(), null, null);
                }
            }
        }

    }
}
