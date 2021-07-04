using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory.MechChips
{
    public class HediffComp_EXT1Guardian : HediffComp
    {
        private int ticks = 0;
        private Abilities.GuardianShield guardianShield;
        private int shieldCooldown = 100;
        public readonly int shieldCooldownBase = 300;
        private float absorbedDamage = 0;

        public int ShieldCooldown { get => shieldCooldown; set => shieldCooldown = value; }
        public float AbsorbedDamage { get => absorbedDamage; set => absorbedDamage = value; }

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
            if (!ModsConfig.RoyaltyActive) return;
            if (Pawn?.Destroyed != false || !Pawn.Awake() || Pawn?.Map == null) return;

            if (ticks >= 1000)
            {
                ticks = 0;
            }

            if (shieldCooldown == 0) if (guardianShield == null || guardianShield.Destroyed || !guardianShield.Spawned) guardianShield = CreateShield();

            ticks++;
            shieldCooldown--;
        }

        private Abilities.GuardianShield CreateShield()
        {
            var thing = ThingMaker.MakeThing(ThingDef.Named("CAGuardianShield")) as Abilities.GuardianShield;
            thing.Owner = Pawn;
            GenSpawn.Spawn(thing, Pawn.Position, Pawn.Map);
            return thing;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
           // cleanup
        }

       
    }
}
