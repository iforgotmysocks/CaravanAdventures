using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class RapidLaser
    {
        private readonly Thing target;
        private readonly Pawn instigator;
        private readonly int frequencyTicks;
        private int randomDelayUpTo;
        private readonly int damage;
        private bool ignoreLineOfSight;
        private int ticks = 0;
        private int totalTicks;
        public bool done = false;
        private IntVec3 offset;

        public RapidLaser(Thing target, Pawn instigator, int shotCount = 10, int frequencyTicks = 15, int randomDelayUpTo = 0, int damage = 7, int offsetX = 0, int offsetZ = 0, bool ignoreLineOfSight = false)
        {
            this.randomDelayUpTo = Rand.Range(0, randomDelayUpTo);
            totalTicks = (shotCount * frequencyTicks);
            this.target = target;
            this.instigator = instigator;
            this.frequencyTicks = frequencyTicks;
            this.damage = damage;
            this.ignoreLineOfSight = ignoreLineOfSight;
            this.offset = new IntVec3(offsetX, 0, offsetZ);
        }

        public void Tick()
        {
            if (randomDelayUpTo > 0)
            {
                ticks--;
                randomDelayUpTo--;
            }
            if (ticks > totalTicks)
            {
                this.done = true;
                return;
            }

            if (ticks % frequencyTicks == 0) LaserAttack();
            ticks++;
        }

        protected virtual bool LaserAttack()
        {
            if (target != null && target != LocalTargetInfo.Invalid && target.Position.DistanceTo(instigator.Position) >= 3f)
            {
                if (!GenGrid.InBounds(target.Position, instigator.Map))
                {
                    DLog.Message($"Target cell out of bounds!");
                    return false;
                }

                var targetPawn = target as Pawn;

                if (target == null) return false;
                if (targetPawn != null && targetPawn.Dead) return false;

                var mote = (RapidLaserMote)ThingMaker.MakeThing(ThingDef.Named("CARapidLaser"));
                mote.beamSize = 0.1f;
                mote.launchPos = instigator.DrawPos;
                mote.launchObject = instigator;
                mote.targetPos = target.DrawPos;
                mote.targetObject = target;
                mote.color = Color.red;
                mote.offset = offset;
                GenSpawn.Spawn(mote, instigator.Position, instigator.Map, WipeMode.Vanish);

                if (targetPawn != null) targetPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, damage, 0.2f, -1, instigator, targetPawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside)));
                else target.TakeDamage(new DamageInfo(DamageDefOf.Crush, 4, 0.5f, -1, instigator));

                return true;
            }
            return false;
        }



    }
}
