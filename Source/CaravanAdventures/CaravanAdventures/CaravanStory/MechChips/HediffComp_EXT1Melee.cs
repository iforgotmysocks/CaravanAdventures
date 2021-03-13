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
    public class HediffComp_EXT1Melee : HediffComp
    {
        private int ticks = 1250;
        private Abilities.CirclingBladesMote blades;

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
            
            if (ticks % 25 == 0 && blades != null && blades.Spawned)
            {
                SliceSurroundingEnemies();
            }

            if (ticks % 350 == 0)
            {
                if (GenRadial.RadialCellsAround(Pawn.Position, 4, false).Select(x => x.GetFirstPawn(Pawn.Map)).Where(x => x != Pawn).Count() != 0)
                {
                    blades = (Abilities.CirclingBladesMote)ThingMaker.MakeThing(ThingDef.Named("CACirclingBlades"));
                    blades.Attach(Pawn);
                    blades.launchObject = Pawn;
                    blades.rotationRate = 1400;
                    //blades.Scale = 2.5f;
                    blades.Scale = 5f;
                    blades.exactPosition = Pawn.DrawPos;
                    blades.solidTimeOverride = -1f;
                    GenSpawn.Spawn(blades, Pawn.Position, Pawn.Map);
                }
            }
            
            if (ticks >= 700)
            {
                if (JumpToTarget()) ticks = 0;
            }
            ticks++;
        }

        protected void SliceSurroundingEnemies()
        {
            var cells = GenRadial.RadialCellsAround(Pawn.Position, 4, false).Where(cell => cell.Standable(Pawn.Map));
            var pawns = cells.SelectMany(cell => cell.GetThingList(Pawn.Map).OfType<Pawn>().Where(pawn => !pawn.RaceProps.IsMechanoid)).ToList();
            if (pawns != null && pawns.Count() != 0)
            {
                foreach (var cell in cells)
                {
                    //    MoteMaker.MakeStaticMote(cell, Pawn.Map, ThingDef.Named("Mote_BlastFlame"));
                    //    MoteMaker.ThrowDustPuffThick(cell.ToVector3(), Pawn.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
                    //    MoteMaker.ThrowAirPuffUp(cell.ToVector3(), Pawn.Map);

                    //var cellPawns = cell.GetThingList(Pawn.Map).OfType<Pawn>().ToList();
                    //cellPawns.ForEach(pawn => pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 20, 50, -1, Pawn, pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside))));
                }
                pawns.ForEach(pawn => Enumerable.Range(0, 3).ToList().ForEach(run => pawn.TakeDamage(new DamageInfo(DamageDefOf.Scratch, 7, 0.5f, -1, Pawn, pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside)))));
                SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                //SoundDef.Named("DropPod_Leaving").PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            }
        }

        protected bool JumpToTarget()
        {
            if (Pawn.mindState.enemyTarget != null 
                && Pawn.mindState.enemyTarget.Position != LocalTargetInfo.Invalid 
                && Pawn.mindState.enemyTarget.Position.DistanceTo(Pawn.Position) >= 2f 
                && Pawn.mindState.enemyTarget.Position.DistanceTo(Pawn.Position) <= 10f)
            {
                var map = Pawn.Map;
                try
                {
                    PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, Pawn, Pawn.mindState.enemyTarget.Position);
                    if (pawnFlyer != null)
                    {
                        GenSpawn.Spawn(pawnFlyer, Pawn.mindState.enemyTarget.Position, map, WipeMode.Vanish);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Log.Message(e.ToString());
                }
            }
            return false;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (blades?.Spawned == true) blades.Destroy();
        }
    }
}
