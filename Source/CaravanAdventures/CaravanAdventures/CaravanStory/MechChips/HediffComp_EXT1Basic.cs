using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory.MechChips
{
    public class HediffComp_EXT1Basic : HediffComp
    {
        private int ticks = 0;
        private Thing light;
        private List<Pawn> producedMechs;

        public new HediffCompProperties_EXT1Basic Props => (HediffCompProperties_EXT1Basic)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            producedMechs = new List<Pawn>();
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            producedMechs = new List<Pawn>();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.Awake()) return;
            if (ticks % 300 == 0)
            {
                if (GenHostility.AnyHostileActiveThreatTo(Pawn.Map, Pawn.Faction))
                {
                    var spawnPos = GetMinionSpawnPosition(Pawn.Position, Pawn.Map);
                    if (spawnPos != null)
                    {
                        var scyther = PawnGenerator.GeneratePawn(PawnKindDef.Named("Mech_Scyther"), Faction.OfMechanoids);
                        GenSpawn.Spawn(scyther, spawnPos, Pawn.Map, WipeMode.Vanish);
                        Pawn.GetLord().AddPawn(scyther);
                        producedMechs.Add(scyther);
                    }

                }
            }

            if (ticks >= 2500)
            {
                try
                {
                    // todo get all targets in the desired range that are in sight, calculate the one with the most bystanding enemies
                    if (Pawn?.TargetCurrentlyAimingAt != null && Pawn.TargetCurrentlyAimingAt != LocalTargetInfo.Invalid && Pawn.TargetCurrentlyAimingAt.Cell.DistanceTo(Pawn.Position) >= 10f)
                    {
                        if (!GenGrid.InBounds(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map))
                        {
                            Log.Message($"Target cell out of boudns!");
                            return;
                        }
                        StoryUtility.CallBombardment(Pawn.TargetCurrentlyAimingAt.Cell, Pawn.Map, Pawn);
                        ticks = 0;
                    }
                }
                catch (Exception e)
                {
                    Log.Message(e.ToString());
                }
               
            }
            ticks++;
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

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref light, "light", null);
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }
    }
}
