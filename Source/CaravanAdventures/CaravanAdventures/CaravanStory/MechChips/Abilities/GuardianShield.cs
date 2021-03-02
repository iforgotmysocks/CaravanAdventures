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
    class GuardianShield : ThingWithComps
    {
        public Pawn Wearer { get => wearer; set => wearer = value; }

        private float EnergyMax
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
            }
        }

        private float EnergyGainPerTick
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
            Scribe_References.Look(ref wearer, "wearer");
        }

        public float GetSpecialApparelScoreOffset()
        {
            return this.EnergyMax * this.ApparelScorePerEnergyMax;
        }

        public override void Tick()
        {
            base.Tick();
            if (Wearer == null)
            {
                this.energy = 0f;
                return;
            }
            if (this.ShieldState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                    return;
                }
            }
            else if (this.ShieldState == ShieldState.Active)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
            if (this.Position != wearer.Position) this.Position = wearer.Position;
        }

        public bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (this.ShieldState != ShieldState.Active)
            {
                return false;
            }
            if (dinfo.Def == DamageDefOf.EMP)
            {
                this.energy = 0f;
                this.Break();
                return false;
            }
            if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                this.energy -= dinfo.Amount * this.EnergyLossPerDamage;
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                return true;
            }
            return false;
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
            MoteMaker.MakeStaticMote(Wearer.TrueCenter(), Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                MoteMaker.ThrowDustPuff(Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
        }

        private void Reset()
        {
            if (Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
                MoteMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }

        public override void Draw()
        {
            base.Draw();
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
                Vector3 vector = Wearer.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        //public override void DrawWornExtras()
        //{
            
        //}

        //public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb)
        //{
        //    return !(verb is Verb_LaunchProjectile) || ReachabilityImmediate.CanReachImmediate(root, targ, map, PathEndMode.Touch, null);
        //}

        private float energy;

        private int ticksToReset = -1;

        private int lastKeepDisplayTick = -9999;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private const float MinDrawSize = 1.2f;

        private const float MaxDrawSize = 1.55f;

        private const float MaxDamagedJitterDist = 0.05f;

        private const int JitterDurationTicks = 8;

        private int StartingTicksToReset = 3200;

        private float EnergyOnReset = 0.2f;

        private float EnergyLossPerDamage = 0.033f;

        private int KeepDisplayingTicks = 1000;

        private float ApparelScorePerEnergyMax = 0.25f;
        private Pawn wearer;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
    }
}
