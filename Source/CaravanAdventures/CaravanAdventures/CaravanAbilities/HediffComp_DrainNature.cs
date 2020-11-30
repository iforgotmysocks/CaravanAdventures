using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_DrainNature : HediffComp
    {
        private int ticks;
        private int ticksSinceLastRadiusIncrease;
        private float currentRadius = 1f;
        private int ticksToPlantHarm = 1;
        // todo - remove totalPlantScore, was just for debugging purposes
        private float plantScore = 0;
        private float totalPlantScore = 0;
        private float skillGainPerTick;

        private readonly float leaflessPlantKillChance = 0.05f;
        private readonly float radiusIncreaseInTicks = 10f;
        private readonly float harmFrequencyPerArea = 1f;
        private readonly float maxRadius = 20;
        private readonly float endTicks = 300;

        private bool isGifted;

        public HediffComp_DrainNature()
        {
          
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            skillGainPerTick = ModSettings.psyfocusToRestore / (300 - 100);

             isGifted = IsGifted;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticks++;

            if (ticks > 100)
            {
                Pawn.psychicEntropy.OffsetPsyfocusDirectly(isGifted ? (skillGainPerTick + plantScore) : ((skillGainPerTick + plantScore) / 2));
                totalPlantScore += plantScore;
                plantScore = 0;
            }

            if (ticks > endTicks)
            {
                //Log.Message("Plantscore: " + totalPlantScore);
                Pawn.health.hediffSet.hediffs.Remove(parent);
                return;
            }
            // changed from ticks % radiusIncreaseInTicks == 0, see if there is any difference in execution
            if (ticksSinceLastRadiusIncrease >= radiusIncreaseInTicks && currentRadius <= maxRadius)
            {
                currentRadius += 1;
                ticksSinceLastRadiusIncrease = 0;
            }
            ticksSinceLastRadiusIncrease++;
         
            ticksToPlantHarm--;
            if (ticksToPlantHarm <= 0)
            {
                float num = 3.14159274f * currentRadius * currentRadius * harmFrequencyPerArea;
                float num2 = 60f / num;
                int num3;
                if (num2 >= 1f)
                {
                    ticksToPlantHarm = GenMath.RoundRandom(num2);
                    num3 = 1;
                }
                else
                {
                    ticksToPlantHarm = 1;
                    num3 = GenMath.RoundRandom(1f / num2);
                }

                for (int i = 0; i < num3; i++)
                {
                    HarmRandomPlantInRadius(currentRadius);
                }
            }
        }
    
        private void HarmRandomPlantInRadius(float radius)
        {
            IntVec3 c = this.parent.pawn.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            if (!c.InBounds(this.parent.pawn.Map))
            {
                return;
            }
            Plant plant = c.GetPlant(this.parent.pawn.Map);
            if (plant != null)
            {
                if (plant.LeaflessNow)
                {
                    if (Rand.Value < leaflessPlantKillChance)
                    {
                        plant.Kill(null, null);
                        plantScore += 0.0004f;
                        return;
                    }
                }
                else
                {
                    plant.MakeLeafless(Plant.LeaflessCause.Poison);
                    plantScore += 0.0002f;
                }
            }
        }

        private bool IsGifted => Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "AncientGift") != null || false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref currentRadius, "currentRadius", 1f);
            Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 1);
            Scribe_Values.Look(ref plantScore, "plantScore", 0);
            Scribe_Values.Look(ref skillGainPerTick, "skillGainPerTick", 0f);
        }
    }
}
