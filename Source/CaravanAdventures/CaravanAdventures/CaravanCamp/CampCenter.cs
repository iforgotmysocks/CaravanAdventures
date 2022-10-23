using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public class CampCenter : CampArea, IRecipeHolder
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

            if (ModSettings.createCampPackingSpot)
            {
                var spot = DefDatabase<ThingDef>.GetNamedSilentFail("CaravanPackingSpot") ?? DefDatabase<ThingDef>.GetNamedSilentFail("CaravanHitchingPost");
                if (spot != null)
                {
                    location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 1);
                    CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(spot, spot.defName == "CaravanHitchingPost" ? ThingDefOf.WoodLog : null), location, map, default, campAssetListRef);
                }
            }

            foreach (var cell in CellRect.Cells) map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            control = CampHelper.PrepAndGenerateThing(CampDefOf.CACampControl, this.CellRect.CenterCell, map, default, campAssetListRef) as ThingWithComps;

            campFire = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CACampfireTribalRoast), this.CellRect.CenterCell, map, Rot4.South, campAssetListRef) as Building_WorkTable;
            var gatherSpotComp = campFire?.TryGetComp<CompGatherSpot>();
            if (gatherSpotComp != null) gatherSpotComp.Active = true;

            var location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 2 && cell.z == CellRect.maxZ);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDef.Named("HorseshoesPin"), ThingDefOf.WoodLog), location, map, default, campAssetListRef);

            if (ModSettings.createCampPackingSpot)
            {
                var spot = DefDatabase<ThingDef>.GetNamedSilentFail("CaravanPackingSpot") ?? DefDatabase<ThingDef>.GetNamedSilentFail("CaravanHitchingPost");
                if (spot != null)
                {
                    location = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.maxZ - 1);
                    CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(spot, spot.defName == "CaravanHitchingPost" ? ThingDefOf.WoodLog : null), location, map, default, campAssetListRef);
                }
            }

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
            if (ModSettings.campStorageAndJobsAllowHumanMeat) bill.ingredientFilter.SetAllow(ThingDefOf.Meat_Human, true);
            if (ModSettings.campStorageAndJobsAllowInsectMeat) bill.ingredientFilter.SetAllow(ThingDef.Named("Meat_Megaspider"), true);

            if (DefDatabase<ResearchProjectDef>.GetNamed("PackagedSurvivalMeal", false)?.ProgressPercent == 1f)
            {
                var surv = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("CookMealSurvivalBulk", false)) { targetCount = colonists.Count != 0 ? colonists.Count * 10 : 12, repeatMode = BillRepeatModeDefOf.TargetCount };
                campFire.BillStack.AddBill(surv);
                if (ModSettings.campStorageAndJobsAllowHumanMeat) surv.ingredientFilter.SetAllow(ThingDefOf.Meat_Human, true);
                if (ModSettings.campStorageAndJobsAllowInsectMeat) surv.ingredientFilter.SetAllow(ThingDef.Named("Meat_Megaspider"), true);
            }

            if (!ModSettings.campStorageAndJobsAllowHumanMeat) return;
            var burnApparel = new Bill_Production(DefDatabase<RecipeDef>.GetNamedSilentFail("BurnApparel")) { repeatMode = BillRepeatModeDefOf.Forever };
            burnApparel.ingredientFilter.SetAllow(SpecialThingFilterDefOf.AllowNonDeadmansApparel, false);
            campFire.billStack.AddBill(burnApparel);
        }

        public virtual void ApplyRecipesTribal(Caravan caravan)
        {
            var colonists = caravan.PawnsListForReading.Where(col => col.IsFreeColonist).ToList();
            var bill = new Bill_Production(CampDefOf.CACookGrillSnackBulk) { targetCount = colonists.Count != 0 ? colonists.Count * 3 : 12, repeatMode = BillRepeatModeDefOf.TargetCount };
            campFire.BillStack.AddBill(bill);
            if (ModSettings.campStorageAndJobsAllowHumanMeat) bill.ingredientFilter.SetAllow(ThingDefOf.Meat_Human, true);
            if (ModSettings.campStorageAndJobsAllowInsectMeat) bill.ingredientFilter.SetAllow(ThingDef.Named("Meat_Megaspider"), true);

            if (DefDatabase<ResearchProjectDef>.GetNamed("Pemmican", false)?.ProgressPercent == 1f)
            {
                var surv = new Bill_Production(DefDatabase<RecipeDef>.GetNamed("Make_PemmicanBulk", false)) { targetCount = colonists.Count != 0 ? colonists.Count * 10 * 18 : 12 * 18, repeatMode = BillRepeatModeDefOf.TargetCount };
                campFire.BillStack.AddBill(surv);
                if (ModSettings.campStorageAndJobsAllowHumanMeat) surv.ingredientFilter.SetAllow(ThingDefOf.Meat_Human, true);
                if (ModSettings.campStorageAndJobsAllowInsectMeat) surv.ingredientFilter.SetAllow(ThingDef.Named("Meat_Megaspider"), true);
            }

            if (!ModSettings.campStorageAndJobsAllowHumanMeat) return;
            var burnApparel = new Bill_Production(DefDatabase<RecipeDef>.GetNamedSilentFail("BurnApparel")) { repeatMode = BillRepeatModeDefOf.Forever };
            burnApparel.ingredientFilter.SetAllow(SpecialThingFilterDefOf.AllowNonDeadmansApparel, false);
            campFire.billStack.AddBill(burnApparel);
        }
    }
}
