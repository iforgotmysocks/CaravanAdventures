using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientGift : HediffComp
    {
        private int ticks = 0;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticks >= 600)
            {
                Pawn.psychicEntropy.OffsetPsyfocusDirectly(0.0035f);
                ticks = 0;
            }
            ticks++;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }
    }
}
