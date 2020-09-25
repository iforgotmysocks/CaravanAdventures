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

        public HediffCompProperties_ConjuredLight Props => (HediffCompProperties_ConjuredLight)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (light != null) light.Destroy();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticksToDisappear > ModSettings.Get().lightDuration) Pawn.health.RemoveHediff(this.parent);
            if (ticks >= 10 && light?.Position != Pawn.Position)
            {
                if (light != null) light.Destroy();
                light = GenSpawn.Spawn(ThingDef.Named("CAMagicLight"), Pawn.Position, Pawn.Map);
                ticks = 0;
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
