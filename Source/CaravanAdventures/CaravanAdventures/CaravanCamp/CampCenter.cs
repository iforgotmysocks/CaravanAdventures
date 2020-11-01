using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CampCenter : CampArea
    {
        public CampCenter()
        {
            SupplyCost = 1;
        }

        public override void Build(Map map)
        {
            GenSpawn.Spawn(CampDefOf.CACampControl, this.CellRect.CenterCell, map);
            GenSpawn.Spawn(ThingDefOf.Campfire, this.CellRect.CenterCell, map);

            foreach (var cornerCell in CellRect.Corners)
            {
                var thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.TorchLamp);
                GenSpawn.Spawn(thing, cornerCell, map, Rot4.South);
            }
        }
    }
}
