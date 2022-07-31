﻿using System;
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
        private float swirlRadius = 3f;

        private List<Swirly> swirlies = new List<Swirly>(); 
        public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + ((int)ModSettings.lightDuration - ticksToDisappear).ToStringTicksToPeriod(true, true);
        public HediffCompProperties_ConjuredLight Props => (HediffCompProperties_ConjuredLight)props;
        public override string CompTipStringExtra => base.CompTipStringExtra + $"Caravan travel speed: x{Math.Round(ModSettings.magicLightCaravanSpeedMult, 1)}";

        public override void CompPostMake() => GenerateSwirlies();

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
                fleckTicks = 0;

                if (swirlies.Count == 0) GenerateSwirlies();
                // todo ensure new pos is actually a little distance from the current position
                var newPos = GetNewPointOnPlayerPath();

                // todo calculate and adjust next fleckTick to update position based on range between positions of current and new swirlPoint
                swirlPoint = newPos;
            }

            foreach (var swirly in swirlies) swirly.Swirl();

            fleckTicks++;
            ticks++;
            ticksToDisappear++;
        }

        private void GenerateSwirlies()
        {
            for (int i = 0; i < Rand.Range(30, 35); i++) swirlies.Add(new Swirly(this, 120, Rand.Range(0, 50)));
        }

        public Vector3 swirlPoint;

        private Vector3 GetNewRandomPointAroundPlayer() => Pawn.DrawPos + new Vector3(Rand.Range(-swirlRadius, swirlRadius), 0, Rand.Range(-swirlRadius, swirlRadius));

        public Vector3 GetNewPointOnPlayerPath()
        {
            IntVec3? node = null;
            var path = Pawn?.pather?.curPath;
            if (path != null) node = path?.Peek(path.NodesLeftCount - 1);
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
