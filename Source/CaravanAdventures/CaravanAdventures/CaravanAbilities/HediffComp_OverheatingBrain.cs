using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_OverheatingBrain : HediffComp
    {
        private int ticks = 0;
        private int ticksToOverheat;

        public HediffCompProperties_OverheatingBrain Props => (HediffCompProperties_OverheatingBrain)props;

        public override string CompLabelInBracketsExtra => (ticksToOverheat - ticks).ToStringTicksToPeriod(true, true, true, true);
        
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref ticksToOverheat, "ticksToOverheat");
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.ticksToOverheat = Props.lifeTimeInSeconds.RandomInRange * 60;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.Dead && ticks >= ticksToOverheat)
            {
                if (Pawn?.Faction == Faction.OfPlayer && Pawn?.RaceProps?.IsMechanoid == true) Pawn.forceNoDeathNotification = true;
                Pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 5000, 200, -1, null, Pawn.health.hediffSet.GetBrain()));
            }
            ticks++;
        }

    }
}
