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
        private Abilities.GuardianShieldPet guardianShield;

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
            base.CompPostTick(ref severityAdjustment);
            if (Pawn?.Destroyed != false || !Pawn.Awake() || Pawn?.Map == null) return;

            if (ticks % 250 == 0)
            {
                //EnsureShield();
                if (guardianShield == null || guardianShield.Destroyed || !guardianShield.Spawned) guardianShield = CreateShield();
            }

            if (ticks % 450 == 0)
            {
            }
            
            if (ticks >= 500)
            {
                ticks = 0;
            }
            ticks++;
        }

        private void EnsureShield()
        {
            if (Pawn.Map == null) return;
            if (!Pawn.apparel.WornApparel.Any(x => x.def.defName == "CAGuardianShieldApparel"))
            {
                DLog.Message($"trying to apply apparel to {Pawn.Name}");
                StoryUtility.EquipApparel(Pawn, ThingDef.Named("CAGuardianShieldApparel"));
            }
        }

        private Abilities.GuardianShieldPet CreateShield()
        {
            var thing = ThingMaker.MakeThing(ThingDef.Named("CAGuardianShieldPet")) as Abilities.GuardianShieldPet;
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
