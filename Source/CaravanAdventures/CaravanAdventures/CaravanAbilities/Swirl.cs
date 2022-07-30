using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    internal class Swirly
    {
        HediffComp_ConjuredLight parent;
        private Vector3 currentPos;
        float size;
        int ticks = 0;
        private int swirlTick;
        private int swirlDelay;
        private float swirlRadius = 0.25f;

        public Swirly(HediffComp_ConjuredLight parent, int swirlTick, int swirlDelay)
        {
            this.parent = parent;
            this.swirlTick = swirlTick;
            this.swirlDelay = swirlDelay;
            this.size = Rand.Range(0.3f, 0.6f); // Rand.Range(0.05f, 0.1f);
        }

        public void Swirl()
        {
            if (swirlDelay > 0)
            {
                swirlDelay--;
                return;
            }
            if (ticks > swirlTick)
            {
                ticks = 0;
                if (currentPos == default) currentPos = parent.Pawn.DrawPos;
                var newPoint = GetNewPointOnPlayerPath();
                MoveFromCurrentPos(newPoint);
                currentPos = newPoint;
            }
            ticks++;
        }
        private void MoveFromCurrentPos(Vector3 vector)
        {
            if (!vector.ShouldSpawnMotesAt(parent.Pawn.Map)) return;
            var def = DefDatabase<FleckDef>.GetNamedSilentFail($"ConjuredLightFleckSmallRotate{Rand.RangeInclusive(2,3)}");
            def.growthRate = Rand.Range(Math.Min(Math.Abs(-0.1f), Math.Abs(size)) * -1 + 0.02f, 0.03f);

            float speed = Rand.Range(1.8f, 2.6f);
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(currentPos, parent.Pawn.Map, def, size);
            dataStatic.rotationRate = (float)Rand.Range(-300, 300);
            dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
            dataStatic.velocitySpeed = speed;
            dataStatic.airTimeLeft = new float?((float)Mathf.RoundToInt((dataStatic.spawnPosition - vector).MagnitudeHorizontal() / speed));
            if (dataStatic.def.solidTime > dataStatic.airTimeLeft) dataStatic.solidTimeOverride = dataStatic.airTimeLeft;
            parent.Pawn.Map.flecks.CreateFleck(dataStatic);
        }

        //private Vector3 GetNewRandomPointAroundPlayer() => GenRadial.RadialCellsAround(parent.swirlPoint, 1, false).RandomElement().ToVector3Shifted();

        private Vector3 GetSwirlPointAroundNewLocation() => parent.swirlPoint + new Vector3(Rand.Range(-swirlRadius, swirlRadius), 0, Rand.Range(-swirlRadius, swirlRadius));

        public Vector3 GetNewPointOnPlayerPath()
        {
            var node = parent.Pawn?.pather?.curPath?.FirstNode;
            if (node == null || !node.HasValue) return GetSwirlPointAroundNewLocation();
            return GenRadial.RadialCellsAround(node.Value, 2, false).RandomElement().ToVector3Shifted();
        }
    }
}
