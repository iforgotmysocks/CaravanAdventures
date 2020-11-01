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
        private ThingWithComps control;
        public ThingWithComps Control { get => control; private set => control = value; }
        public CampCenter()
        {
            SupplyCost = 1;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            control = GenSpawn.Spawn(CampDefOf.CACampControl, this.CellRect.CenterCell, map) as ThingWithComps;
            campAssetListRef.Add(control);
            campAssetListRef.Add(GenSpawn.Spawn(ThingDefOf.Campfire, this.CellRect.CenterCell, map));

            foreach (var cornerCell in CellRect.Corners)
            {
                var thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.TorchLamp);
                campAssetListRef.Add(GenSpawn.Spawn(thing, cornerCell, map, Rot4.South));
            }
        }
    }
}
