using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class FoodTent : Tent
    {
        public FoodTent()
        {
            this.CoordSize = 2;
        }

        public override void Build(Map map)
        {
            base.Build(map);
            var cacoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var cacooler = GenSpawn.Spawn(CampThingDefOf.CACooler, cacoolerPos, map);
            cacooler.SetFaction(Faction.OfPlayer);
            var refuelComp = cacooler.TryGetComp<CompRefuelable>();
            // todo check if caravan has fuel
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());
        }
    }
}
