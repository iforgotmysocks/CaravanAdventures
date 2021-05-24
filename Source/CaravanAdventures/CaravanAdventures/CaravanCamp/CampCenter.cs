using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CampCenter : CampArea, IRecipeHolder
    {
        protected ThingWithComps control;
        public ThingWithComps Control { get => control; private set => control = value; }
        protected Building_WorkTable campFire;
        public CampCenter()
        {
            SupplyCost = 1;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            control = CampHelper.PrepAndGenerateThing(CampDefOf.CACampControl, this.CellRect.CenterCell, map, default, campAssetListRef) as ThingWithComps;

            campFire = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CACampfireRoast), this.CellRect.CenterCell, map, Rot4.South, campAssetListRef) as Building_WorkTable;
            var gatherSpotComp = campFire?.TryGetComp<CompGatherSpot>();
            if (gatherSpotComp != null) gatherSpotComp.Active = true;

            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX && cell.z == CellRect.minZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Table1x2c"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.minZ);
            CampHelper.PrepAndGenerateThing(ThingDefOf.TorchLamp, location, map, default, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.maxZ - 1);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 3 && cell.z == CellRect.minZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Table1x2c"), ThingDefOf.WoodLog), location, map, Rot4.East, campAssetListRef);

            CellRect.Cells.Where(cell => cell.z == CellRect.minZ + 1 && cell.x != CellRect.minX + 2).ToList().ForEach(cell =>
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), cell, map, default, campAssetListRef));

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Column"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX && cell.z == CellRect.minZ + 2);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("Column"), ThingDefOf.WoodLog), location, map, Rot4.North, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 1 && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("ChessTable"), ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HorseshoesPin"), ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            foreach (var cell in CellRect.Cells) map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            control = CampHelper.PrepAndGenerateThing(CampDefOf.CACampControl, this.CellRect.CenterCell, map, default, campAssetListRef) as ThingWithComps;

            campFire = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Campfire), this.CellRect.CenterCell, map, Rot4.South, campAssetListRef) as Building_WorkTable;
            var gatherSpotComp = campFire?.TryGetComp<CompGatherSpot>();
            if (gatherSpotComp != null) gatherSpotComp.Active = true;

            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HorseshoesPin"), ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            foreach (var cornerCell in CellRect.Corners)
            {
                var thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.TorchLamp);
                thing.SetFaction(Faction.OfPlayer);
                campAssetListRef.Add(GenSpawn.Spawn(thing, cornerCell, map, Rot4.South));
            }
        }

        public virtual void ApplyRecipes(Caravan caravan)
        {
            var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
            var bill = new Bill_Production(CampDefOf.CACookGrillSnackBulk) { targetCount = colonists.Count != 0 ? colonists.Count * 3 : 12, repeatMode = BillRepeatModeDefOf.TargetCount };
            campFire.BillStack.AddBill(bill);
        }

        public virtual void ApplyRecipesTribal(Caravan caravan)
        {
            var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
            var bill = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("CookMealSimpleBulk")) { targetCount = colonists.Count != 0 ? colonists.Count * 3 : 12, repeatMode = BillRepeatModeDefOf.TargetCount };
            campFire.BillStack.AddBill(bill);
        }
    }
}
