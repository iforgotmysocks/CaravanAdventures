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
    public class HediffComp_EXT1Basic : HediffComp
    {
        private int ticks = 0;
        private List<Pawn> producedMechs;

        public HediffCompProperties_EXT1Basic Props => (HediffCompProperties_EXT1Basic)props;

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
            if (Pawn?.Destroyed != false || !Pawn.Awake()) return;

            if (ticks % 250 == 0)
            {
                ManufactureScyther();
            }

            if (ticks % 450 == 0)
            {
                ExpanseBurningSteam(8);
            }
            
            if (ticks >= 900)
            {
                if (LaunchBombardmentOnCurrentTarget()) ticks = 0;
            }
            ticks++;
        }

        protected void ManufactureScyther()
        {
            if (producedMechs.Where(x => !x.Dead).Count() > 15) return;
            if (GenHostility.AnyHostileActiveThreatTo(Pawn.Map, Pawn.Faction))
            {
                var spawnPos = GetMinionSpawnPosition(Pawn.Position, Pawn.Map);
                if (spawnPos != null)
                {
                    var scyther = PawnGenerator.GeneratePawn(PawnKindDef.Named("Mech_Scyther"), Faction.OfMechanoids);
                    scyther.health.AddHediff(HediffDef.Named("CAOverheatingBrain"), scyther.health.hediffSet.GetBrain());
                    GenSpawn.Spawn(scyther, spawnPos, Pawn.Map, WipeMode.Vanish);
                    Pawn.GetLord().AddPawn(scyther);
                    producedMechs.Add(scyther);
                }
            }
        }

        protected bool LaunchBombardmentOnCurrentTarget()
        {
            // todo get all targets in the desired range that are in sight, calculate the one with the most bystanding enemies
            if (Pawn?.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt != LocalTargetInfo.Invalid && Pawn.TargetCurrentlyAimingAt.Cell.DistanceTo(Pawn.Position) >= 8f)
            {
                // todo choose different target if current is too close
                if (!GenGrid.InBounds(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map))
                {
                    DLog.Message($"Target cell out of bounds!");
                    return false;
                }
                StoryUtility.CallBombardment(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map, Pawn, 3, 4, 4, 6, 45);
                return true;
            }
            return false;
        }

        protected void ExpanseBurningSteam(int rangeInCells = 6)
        {
            var cells = GenRadial.RadialCellsAround(Pawn.Position, rangeInCells, false).Where(cell => cell.Standable(Pawn.Map));
            var pawns = cells.SelectMany(cell => cell.GetThingList(Pawn.Map).OfType<Pawn>().Where(pawn => !pawn.RaceProps.IsMechanoid)).ToList();
            if (pawns != null && pawns.Count() != 0)
            {
                foreach (var cell in cells)
                {
                    //MoteMaker.MakeStaticMote(cell, Pawn.Map, ThingDef.Named("Mote_LongSparkThrown"));
                    MoteMaker.MakeStaticMote(cell, Pawn.Map, ThingDef.Named("Mote_BlastFlame"));
                    //MoteMaker.MakeStaticMote(cell, Pawn.Map, ThingDef.Named("Mote_DustSlow"));
                    MoteMaker.ThrowDustPuffThick(cell.ToVector3(), Pawn.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
                    MoteMaker.ThrowAirPuffUp(cell.ToVector3(), Pawn.Map);

                    //var cellPawns = cell.GetThingList(Pawn.Map).OfType<Pawn>().ToList();
                    //cellPawns.ForEach(pawn => pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 20, 50, -1, Pawn, pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside))));
                }
                pawns.ForEach(pawn => Enumerable.Range(0, 3).ToList().ForEach(run => pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, 10, 0.5f, -1, Pawn, pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Burn, BodyPartHeight.Undefined, BodyPartDepth.Outside)))));

                SoundDef.Named("DropPod_Leaving").PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
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
