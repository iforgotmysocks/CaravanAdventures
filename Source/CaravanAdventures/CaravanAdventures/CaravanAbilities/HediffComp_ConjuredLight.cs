using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_ConjuredLight : HediffComp
    {
        private int ticks = 0;
        private int ticksToDisappear = 0;
        private Thing light;
        private int fleckTicks;

        private List<Swirly> swirlies = new List<Swirly>(); 
        public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + ((int)ModSettings.lightDuration - ticksToDisappear).ToStringTicksToPeriod(true, true);
        public HediffCompProperties_ConjuredLight Props => (HediffCompProperties_ConjuredLight)props;
        public override string CompTipStringExtra => base.CompTipStringExtra + $"Caravan travel speed: x{Math.Round(ModSettings.magicLightCaravanSpeedMult, 1)}";

        public override void CompPostMake()
        {
            for (int i = 0; i < Rand.Range(20,30); i++) swirlies.Add(new Swirly(this, 120, new IntRange(0, 50).RandomInRange));
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Patches.CaravanMagicLight.magicLightTravelers.Contains(Pawn)) Patches.CaravanMagicLight.magicLightTravelers.Remove(Pawn);
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

            if (fleckTicks > 120 && Pawn?.Spawned == true)
            {
                DLog.Message($"should be running the fleck");
                fleckTicks = 0;

                if (currentPos == default) currentPos = Pawn.DrawPos;
                var newPoint = GetNewPointOnPlayerPath();
                //MoveFromCurrentPos(newPoint);
                currentPos = newPoint;

                swirlPoint = newPoint;
            }

            foreach (var swirly in swirlies) swirly.Swirl();

            fleckTicks++;
            ticks++;
            ticksToDisappear++;
        }
        public Vector3 swirlPoint;
        private Vector3 currentPos;

        private void MoveFromCurrentPos(Vector3 vector)
        {
            if (!currentPos.ShouldSpawnMotesAt(parent.pawn.Map)) return;
            var def = DefDatabase<FleckDef>.GetNamedSilentFail("ConjuredLightFleck");
            def.growthRate = new FloatRange(-0.2f, 0.1f).RandomInRange;

            float speed = Rand.Range(1.8f, 2.6f);
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(currentPos, parent.pawn.Map, def, 0.3f); 
            dataStatic.rotationRate = (float)Rand.Range(-300, 300);
            dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
            dataStatic.velocitySpeed = speed;
            dataStatic.airTimeLeft = new float?((float)Mathf.RoundToInt((dataStatic.spawnPosition - vector).MagnitudeHorizontal() / speed));
            Pawn.Map.flecks.CreateFleck(dataStatic);
        }

        private Vector3 GetNewRandomPointAroundPlayer() => GenRadial.RadialCellsAround(Pawn.Position, 3, false).RandomElement().ToVector3Shifted(); 

        public Vector3 GetNewPointOnPlayerPath()
        {
            var node = Pawn?.pather?.curPath?.FirstNode;
            if (node == null || !node.HasValue) return GetNewRandomPointAroundPlayer();
            return GenRadial.RadialCellsAround(node.Value, 2, false).RandomElement().ToVector3Shifted();
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
