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
        private int ticks = 0;
        private List<Pawn> producedMechs;
        private AbilityMotes.CirclingBlades blades;

        public HediffCompProperties_EXT1Melee Props => (HediffCompProperties_EXT1Melee)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Collections.Look(ref producedMechs, "producedMechs", LookMode.Reference);
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            producedMechs = new List<Pawn>();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn?.Destroyed != false || !Pawn.Awake() || Pawn.Map == null) return;
            
            if (ticks % 25 == 0 && blades != null && blades.Spawned)
            {
                SliceSurroundingEnemies();
            }

            if (ticks % 600 == 0)
            {
                if (GenRadial.RadialCellsAround(Pawn.Position, 2, false).Select(x => x.GetFirstPawn(Pawn.Map)).Count() != 0)
                {
                    blades = (AbilityMotes.CirclingBlades)ThingMaker.MakeThing(ThingDef.Named("CACirclingBlades"));
                    blades.Attach(Pawn);
                    blades.launchObject = Pawn;
                    blades.rotationRate = 1200;
                    blades.Scale = 2.5f;
                    blades.exactPosition = Pawn.DrawPos;
                    blades.solidTimeOverride = -1f;
                    GenSpawn.Spawn(blades, Pawn.Position, Pawn.Map);
                }
            }

            if (ticks % 1250 == 0)
            {
                //SliceSurroundingEnemies();
            }
            
            if (ticks >= 1300)
            {
                if (JumpToTarget()) ticks = 0;
            }
            ticks++;
        }

        protected void SliceEnemiesInfront()
        {
            if (GenHostility.AnyHostileActiveThreatTo(Pawn.Map, Pawn.Faction))
            {
                var spawnPos = GetMinionSpawnPosition(Pawn.Position, Pawn.Map);
                if (spawnPos != default)
                {
                    var scyther = PawnGenerator.GeneratePawn(PawnKindDef.Named("Mech_Scyther"), Faction.OfMechanoids);
                    GenSpawn.Spawn(scyther, spawnPos, Pawn.Map, WipeMode.Vanish);
                    Pawn.GetLord().AddPawn(scyther);
                    producedMechs.Add(scyther);
                }
            }
        }

        

        
        protected bool JumpToTarget()
        {
            // todo get all targets in the desired range that are in sight, calculate the one with the most bystanding enemies
            if (Pawn?.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt != LocalTargetInfo.Invalid && Pawn.TargetCurrentlyAimingAt.Cell.DistanceTo(Pawn.Position) >= 4f)
            {


                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, Pawn, Pawn.Position);
                if (pawnFlyer != null)
                {
                    GenSpawn.Spawn(pawnFlyer, Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map, WipeMode.Vanish);
                    return true;
                }


                //if (!GenGrid.InBounds(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map))
                //{
                //    DLog.Message($"Target cell out of bounds!");
                //    return false;
                //}
                //StoryUtility.CallBombardment(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map, Pawn);
                return true;
            }

          

            return false;
        }

        protected void SliceSurroundingEnemies()
        {
            var cells = GenRadial.RadialCellsAround(Pawn.Position, 2, false).Where(cell => cell.Standable(Pawn.Map));
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
                pawns.ForEach(pawn => Enumerable.Range(0, 3).ToList().ForEach(run => pawn.TakeDamage(new DamageInfo(DamageDefOf.Scratch, 20, 0.5f, -1, Pawn, pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside)))));
                SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                //SoundDef.Named("DropPod_Leaving").PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            }
        }

        private IntVec3 GetMinionSpawnPosition(IntVec3 position, Map map)
        {
            var positions = GenRadial.RadialCellsAround(position, 1, false);
            return positions.Where(x => x.Standable(map)).InRandomOrder().FirstOrDefault();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            foreach (var mech in producedMechs)
            {
                if (mech != null && !mech.Dead) mech.TakeDamage(new DamageInfo(DamageDefOf.Burn, 5000, 200, -1, null, mech.health.hediffSet.GetBrain()));
            }
        }


    }
}
