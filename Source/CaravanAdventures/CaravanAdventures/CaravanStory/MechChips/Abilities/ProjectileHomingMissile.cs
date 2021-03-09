using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class ProjectileHomingMissile : Projectile_Explosive
    {
        private Map map;

        public float rotationSpeed = 10f;
        public float speed = 20f;

        public Vector3 realPosition;
        public Quaternion realRotation;

        public int maxTicks = 20000;
        public int ticksAlive = 0;

        public Vector3 lastPosition = new Vector3();

        private static List<IntVec3> checkedCells = new List<IntVec3>();
        public Vector3 offset = new Vector3(0,0,0);
        public FloatRange effectRange = new FloatRange(0.5f, 1f);
        public float launchSpeed = 10f;
        public int launchTicks = 140;
        public bool smallerMissile = false;

        private Sustainer ambientSustainer;

        public new void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            realRotation = launcher.Rotation.AsQuat;
            realPosition = launcher.DrawPos + (realRotation * offset);

            this.launcher = launcher;
            this.origin = origin;
            this.usedTarget = usedTarget;
            this.intendedTarget = intendedTarget;
            this.targetCoverDef = targetCoverDef;
            this.HitFlags = hitFlags;
            if (equipment != null)
            {
                this.equipmentDef = equipment.def;
                this.weaponDamageMultiplier = equipment.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier, true);
            }
            else
            {
                this.equipmentDef = null;
                this.weaponDamageMultiplier = 1f;
            }
            this.destination = usedTarget.Cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.3f);
            this.ticksToImpact = Mathf.CeilToInt(this.StartingTicksToImpact);
            if (this.ticksToImpact < 1)
            {
                this.ticksToImpact = 1;
            }
            if (!this.def.projectile.soundAmbient.NullOrUndefined())
            {
                SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                this.ambientSustainer = this.def.projectile.soundAmbient.TrySpawnSustainer(info);
            }
        }


        public override void Tick()
        {
            map = usedTarget.Thing?.Map ?? Map;
            if (map == null) {
                return;
            }
            if (ticksAlive % 10 == 0)
            {
                MoteMaker.ThrowDustPuff(Position, map, effectRange.RandomInRange);
                MoteMaker.ThrowFireGlow(Position, map, effectRange.RandomInRange);
            }
            FindTarget();
            UpdateRotationAndPosition();

            if (ticksAlive >= maxTicks || this.Position == usedTarget.Thing?.Position)
            {
                Explode();
                return;
            }
            ticksAlive++;
        }

        private void FindTarget()
        {
            if (usedTarget.Thing == null || usedTarget.ThingDestroyed)
            {
                if (map == null)
                {
                    Log.Message($"map null");
                    return;
                }
                var newTarget = map.mapPawns.AllPawnsSpawned.Where(pawn => (Thing)pawn != this.launcher && !pawn.Dead && pawn.RaceProps.Humanlike).InRandomOrder().FirstOrDefault();
                if (newTarget != null)
                {
                    usedTarget = newTarget;
                    intendedTarget = newTarget;
                }
            }
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
        }

        public override void Draw()
        {
            float num = this.ArcHeightFactor * GenMath.InverseParabola(this.DistanceCoveredFraction);
            Vector3 drawPos = realPosition;
            Vector3 position = realPosition + new Vector3(0f, 0f, 1f) * num;
            if (this.def.projectile.shadowSize > 0f)
            {
                this.DrawShadow(drawPos, num);
            }
            Graphics.DrawMesh(MeshPool.GridPlane(this.def.graphicData.drawSize), position, realRotation, this.def.DrawMatSingle, 0);
            base.Comps_PostDraw();
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            // todo 
            Material shadowmaterial = null;
            if (shadowmaterial == null)
            {
                return;
            }
            float num = this.def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
            Vector3 s = new Vector3(num, 1f, num);
            Vector3 b = new Vector3(0f, -0.01f, 0f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawLoc + b, Quaternion.identity, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, shadowmaterial, 0);
        }

        private float ArcHeightFactor
        {
            get
            {
                float num = this.def.projectile.arcHeightFactor;
                float num2 = (this.destination - this.origin).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f)
                {
                    num = Mathf.Sqrt(num2) * 0.2f;
                }
                return num;
            }
        }

        public void UpdateRotationAndPosition()
        {
            var dir = new Vector3();
            var angle = 0f;
            var rotateToTarget = new Quaternion();
            if (usedTarget.Thing?.Position != null)
            {
                dir = (usedTarget.Thing.DrawPos - realPosition).normalized;
                angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            }
            rotateToTarget = Quaternion.Euler(0, angle, 0);
            realRotation = Quaternion.Slerp(realRotation, rotateToTarget, Time.deltaTime * rotationSpeed);
            var wayAdvanced = Vector3.forward.normalized.Yto0() * (ticksAlive < launchTicks ? launchSpeed : speed) * Time.deltaTime;
            wayAdvanced = realRotation * wayAdvanced;
            realPosition += wayAdvanced;

            if (!CheckStillOnMap(realPosition.ToIntVec3())) return;

            this.Position = realPosition.ToIntVec3();

            //Vector3 exactPosition = this.ExactPosition;
            //this.ticksToImpact--;
            //if (!this.ExactPosition.InBounds(map))
            //{
            //    this.ticksToImpact++;
            //    Position = this.ExactPosition.ToIntVec3();
            //    //this.Destroy(DestroyMode.Vanish);
            //    //return;
            //}
            if (lastPosition == default(Vector3)) lastPosition = realPosition;
            if (CheckForFreeInterceptBetween(lastPosition, realPosition))
            {
                return;
            }
            //Position = this.ExactPosition.ToIntVec3();
            //if (this.ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && this.def.projectile.soundImpactAnticipate != null)
            //{
            //    this.def.projectile.soundImpactAnticipate.PlayOneShot(this);
            //}

            if (ambientSustainer != null)
            {
                ambientSustainer.Maintain();
            }

            lastPosition = realPosition;
        }

        private bool CheckStillOnMap(IntVec3 position)
        {
            if (position.InBounds(map)) return true;
            this.Destroy();
            return false;
        }

        private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            if (lastExactPos == newExactPos)
            {
                return false;
            }
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, lastExactPos, newExactPos))
                {
                    this.Destroy(DestroyMode.Vanish);
                    return true;
                }
            }
            IntVec3 intVec = lastExactPos.ToIntVec3();
            IntVec3 intVec2 = newExactPos.ToIntVec3();
            if (intVec2 == intVec)
            {
                return false;
            }
            if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
            {
                return false;
            }
            if (intVec2.AdjacentToCardinal(intVec))
            {
                return this.CheckForFreeIntercept(intVec2);
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(this.origin, intVec2) <= 0f)
            {
                return false;
            }
            Vector3 vector = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 b = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            checkedCells.Clear();
            int num2 = 0;
            for (; ; )
            {
                vector += b;
                IntVec3 intVec3 = vector.ToIntVec3();
                if (!checkedCells.Contains(intVec3))
                {
                    if (this.CheckForFreeIntercept(intVec3))
                    {
                        break;
                    }
                    checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
                if (intVec3 == intVec2)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (this.destination.ToIntVec3() == c)
            {
                return false;
            }
            float num = VerbUtility.InterceptChanceFactorFromDistance(this.origin, c);
            if (smallerMissile) num /= 2;
            if (num <= 0f)
            {
                return false;
            }
            bool flag = false;
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (this.CanHit(thing))
                {
                    bool flag2 = false;
                    if (thing.def.Fillage == FillCategory.Full)
                    {
                        Building_Door building_Door = thing as Building_Door;
                        if (building_Door == null || !building_Door.Open)
                        {
                            this.ThrowDebugText("int-wall", c);
                            this.Impact(thing);
                            return true;
                        }
                        flag2 = true;
                    }
                    float num2 = 0f;
                    Pawn pawn = thing as Pawn;
                    if (pawn != null)
                    {
                        num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                        if (pawn.GetPosture() != PawnPosture.Standing)
                        {
                            num2 *= 0.1f;
                        }
                        if (this.launcher != null && pawn.Faction != null && this.launcher.Faction != null && !pawn.Faction.HostileTo(this.launcher.Faction))
                        {
                            num2 *= Find.Storyteller.difficultyValues.friendlyFireChanceFactor;
                        }
                    }
                    else if (thing.def.fillPercent > 0.2f)
                    {
                        if (flag2)
                        {
                            num2 = 0.05f;
                        }
                        else if (this.DestinationCell.AdjacentTo8Way(c))
                        {
                            num2 = thing.def.fillPercent * 1f;
                        }
                        else
                        {
                            num2 = thing.def.fillPercent * 0.15f;
                        }
                    }
                    num2 *= num;
                    if (num2 > 1E-05f)
                    {
                        if (Rand.Chance(num2))
                        {
                            this.ThrowDebugText("int-" + num2.ToStringPercent(), c);
                            this.Impact(thing);
                            return true;
                        }
                        flag = true;
                        this.ThrowDebugText(num2.ToStringPercent(), c);
                    }
                }
            }
            if (!flag)
            {
                this.ThrowDebugText("o", c);
            }
            return false;
        }

        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), base.Map, text, -1f);
            }
        }

        protected override void Explode()
        {
            if (this.Destroyed) return;
            this.Destroy(DestroyMode.Vanish);
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(Position, map, false), new TargetInfo(Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = Position;
            Map map2 = map;
            float explosionRadius = this.def.projectile.explosionRadius;
            DamageDef damageDef = this.def.projectile.damageDef;
            Thing launcher = this.launcher;
            int damageAmount = DamageAmount;

            // todo move to explosion
            var hittedPawn = this.usedTarget.Pawn;
            if (hittedPawn != null)
            {
                damageAmount = Convert.ToInt32((float)damageAmount * hittedPawn.BodySize);
            }

            float armorPenetration = ArmorPenetration;
            SoundDef soundExplode = this.def.projectile.soundExplode;
            ThingDef equipmentDef = this.equipmentDef;
            ThingDef def = this.def;
            Thing thing = this.usedTarget.Thing;
            ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = this.def.projectile.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = this.def.projectile.preExplosionSpawnThingCount;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmount, armorPenetration, soundExplode, equipmentDef, def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, this.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff, new float?(this.origin.AngleToFlat(this.destination)), null);
        }

    }
}
