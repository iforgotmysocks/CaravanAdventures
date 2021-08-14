using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_ConjuredLight : HediffComp
    {
        private int ticks = 0;
        private int ticksToDisappear = 0;
        private Thing light;
        public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + ((int)ModSettings.lightDuration - ticksToDisappear).ToStringTicksToPeriod(true, true);

        public HediffCompProperties_ConjuredLight Props => (HediffCompProperties_ConjuredLight)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Patches.CaravanMagicLight.magicLightTravelers.Contains(Pawn))
            {
                Patches.CaravanMagicLight.magicLightTravelers.Remove(Pawn);
            }
            if (light != null) light.Destroy();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
           
            if (ticksToDisappear > ModSettings.lightDuration) Pawn.health.RemoveHediff(this.parent);

            if (ticks > 500)
            {
                ticks = 0;
                if (!Patches.CaravanMagicLight.magicLightTravelers.Contains(Pawn))
                {
                    Patches.CaravanMagicLight.magicLightTravelers.Add(Pawn);
                }
            }

            ticks++;
            ticksToDisappear++;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref light, "light", null);
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
        }
    }
}
