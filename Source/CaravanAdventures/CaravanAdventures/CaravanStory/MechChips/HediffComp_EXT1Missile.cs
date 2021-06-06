using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaravanAdventures.CaravanStory.MechChips.Abilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory.MechChips
{
    public class HediffComp_EXT1Missile : HediffComp
    {
        private int ticks = 0;
        public bool right = true;
        private bool hasTarget = false;

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
            base.CompPostTick(ref severityAdjustment);
            if (Pawn?.Destroyed != false || !Pawn.Awake() || Pawn.Map == null) return;

            if (ticks % 40 == 0) hasTarget = CanLaunchMissile();

            if (ticks > 50 && ticks < 100)
            {
                if (ticks % 5 == 0 && hasTarget)
                {
                    LaunchStinger(right);
                    right = !right;
                }
            }

            if (ticks >= 300)
            {
                ticks = 0;
                //if (hasTarget) LaunchMissile();
                hasTarget = false;
            }
            ticks++;
        }

        private bool CanLaunchMissile()
        {
            //return true;

            //return Pawn.mindState.enemyTarget != null
            //    && Pawn.mindState.enemyTarget.Position != LocalTargetInfo.Invalid
            //    && Pawn.mindState.enemyTarget.Position.DistanceTo(Pawn.Position) >= 0f
            //    && Pawn.mindState.enemyTarget.Position.DistanceTo(Pawn.Position) <= 30f;

            var cells = GenRadial.RadialCellsAround(Pawn.Position, 30, false).Where(cell => cell.Standable(Pawn.Map));
            var pawns = cells.SelectMany(cell => cell.GetThingList(Pawn.Map).OfType<Pawn>().Where(pawn => pawn.RaceProps.Humanlike && !pawn.RaceProps.IsMechanoid)).ToList();
            if (pawns != null && pawns.Count() != 0) return true;

            return false;
        }

        private void LaunchStinger(bool right)
        {
            var target = Pawn.mindState.enemyTarget;
            ProjectileHomingMissile projectile = (ProjectileHomingMissile)GenSpawn.Spawn(ThingDef.Named("CAStingerMissile"), Pawn.Position, Pawn.Map, WipeMode.Vanish);
            projectile.offset = right ? new Vector3(1f, 0, 1) : new Vector3(-1f, 0, 1);
            projectile.pawnOffsetMode = true;
            projectile.speed = 25f;
            projectile.launchSpeed = 4.5f;
            projectile.launchTicks = 50;
            projectile.rotationSpeed = 25f;
            projectile.effectRange = new FloatRange(0.25f, 0.5f);
            projectile.smallerMissile = true;
            projectile.Launch(Pawn, Pawn.DrawPos, target, target, ProjectileHitFlags.All);
        }

        private void LaunchMissile()
        {
            var target = Pawn.mindState.enemyTarget;
            ProjectileHomingMissile projectile = (ProjectileHomingMissile)GenSpawn.Spawn(ThingDef.Named("CAHomingMissile"), Pawn.Position, Pawn.Map, WipeMode.Vanish);
            projectile.realPosition = Pawn.DrawPos;
            projectile.realRotation = Pawn.Rotation.AsQuat;
            projectile.speed = 15f;
            projectile.rotationSpeed = 10f;
            projectile.Launch(Pawn, Pawn.DrawPos, target, target, ProjectileHitFlags.All);
        }


    }
}
