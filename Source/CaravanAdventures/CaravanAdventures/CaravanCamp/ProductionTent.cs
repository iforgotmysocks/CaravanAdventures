using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class ProductionTent : Tent
    {
        public ProductionTent()
        {
            CoordSize = 2;
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            SupplyCost = 4;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("SimpleResearchBench"), ThingDefOf.WoodLog), location, map, Rot4.West, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 4 && cell.z == CellRect.minZ + 3);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableSculpting"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 3);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("TableButcher"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 3 && cell.z == CellRect.minZ + 1);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("ToolCabinet")), location, map, Rot4.South, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HandTailoringBench"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HandTailoringBench"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 5 && cell.z == CellRect.minZ + 1);
            var heater = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampThingDefOf.CAAirConditioningHeater), location, map, default, campAssetListRef);
            var refuelComp = heater.TryGetComp<CompRefuelable>();
            // todo check if caravan has fuel
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());

            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.minX + 4, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 3, 0, CellRect.minZ + 2), map, default, campAssetListRef);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), new IntVec3(CellRect.maxX - 4, 0, CellRect.minZ + 2), map, default, campAssetListRef);
        }
    }
}
