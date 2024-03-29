﻿using System.Linq;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips
{
    public class HediffComp_EXT1Basic : HediffComp
    {
        private int ticks = 0;

        public HediffCompProperties_EXT1Basic Props => (HediffCompProperties_EXT1Basic)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (ticks > 60)
            {
                ticks = 0;
                Helper.RunSafely(NegateNegativeEffects);
                Helper.RunSafely(MakeSureDeadWhenDowned);
            }

            ticks++;
        }

        private IntVec3 GetMinionSpawnPosition(IntVec3 position, Map map)
        {
            var positions = GenRadial.RadialCellsAround(position, 1, false);
            return positions.Where(x => x.Standable(map)).InRandomOrder().FirstOrDefault();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
        }

        public virtual void NegateNegativeEffects()
        {
            if (Pawn?.MentalState?.def == MentalStateDefOf.BerserkMechanoid
                || Pawn?.MentalState?.def == MentalStateDefOf.Berserk)
                Pawn.MentalState.RecoverFromState();

            var shockHediff = Pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.PsychicShock);
            if (shockHediff != null) Pawn.health.RemoveHediff(shockHediff);
        }

        public virtual void MakeSureDeadWhenDowned()
        {
            if (Pawn != null && !Pawn.Dead && Pawn.Downed) Pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 5000, 200, -1, null, Pawn.health.hediffSet.GetBrain()));
        }

    }
}
