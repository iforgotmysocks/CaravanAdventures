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
    public class HediffComp_EXT1Laser : HediffComp
    {
        private int ticks = 1250;
        List<RapidLaser> lasers = new List<RapidLaser>();

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

            TickLasers();

            if (ticks >= 600)
            {
                LaserAttack();
                ticks = 0;
            }
            ticks++;
        }

        protected void TickLasers()
        {
            foreach (var laser in lasers.Reverse<RapidLaser>())
            {
                if (laser.done)
                {
                    lasers.Remove(laser);
                    continue;
                }
                laser.Tick();
            }
        }

        protected virtual void LaserAttack()
        {
            var cells = GenRadial.RadialCellsAround(Pawn.Position, 17, false).Where(cell => cell.InBounds(Pawn.Map));
            var pawns = cells.SelectMany(cell => cell.GetThingList(Pawn.Map).OfType<Pawn>().Where(pawn => !pawn.RaceProps.IsMechanoid)).ToList();

            foreach (var pawn in pawns) lasers.Add(new RapidLaser(pawn, Pawn, 10, 15, 7, 4));
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
        }
    }
}
