using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class RestTent : Tent
    {
        public List<Pawn> Occupants { get; set; }

        public RestTent()
        {
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            Occupants = new List<Pawn>();
            SupplyCost = ModSettings.campSupplyCostRestTent; // 3;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var lover = Occupants.FirstOrDefault(col => col != null && CampHelper.ExistingColonistLovePartner(col, Occupants) != null);
            var otherLover = lover != null ? CampHelper.ExistingColonistLovePartner(lover, Occupants) : null;
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && cell.z == CellRect.maxZ - 1).ToArray();

            for (int i = 0; i < cellSpots.Length; i++)
            {
                if (lover != null && !lover.IsPrisoner && i == 0) continue;
                else if (lover != null && !lover.IsPrisoner && i == 1)
                {
                    var dbThing = ThingMaker.MakeThing(CampDefOf.CASpacerBedrollDouble, CampDefOf.CASpacerTentFabric);
                    var doubleBed = GenSpawn.Spawn(dbThing, cellSpots[i], map, Rot4.South);
                    doubleBed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(doubleBed);
                    lover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    otherLover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                }
                else
                {
                    var thing = ThingMaker.MakeThing(CampDefOf.CASpacerBedroll, CampDefOf.CASpacerTentFabric);
                    var bed = GenSpawn.Spawn(thing, cellSpots[i], map, Rot4.South);
                    bed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(bed);
                    var pawnInNeedOfBed = Occupants.FirstOrDefault(occ => occ != null && occ != lover && occ != otherLover && (occ.ownership.OwnedBed == null || occ.ownership.OwnedBed.Map != map));
                    if (pawnInNeedOfBed != null) pawnInNeedOfBed.ownership.ClaimBedIfNonMedical((Building_Bed)bed);
                }
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var caheater = GenSpawn.Spawn(CampDefOf.CAAirConditioningHeater, caheaterPos, map);
            caheater.SetFaction(Faction.OfPlayer);
            campAssetListRef.Add(caheater);
            CampHelper.RefuelByPerc(caheater, ModSettings.fuelStartingFillPercentage);

            var plantPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 1 && cell.z == CellRect.minZ + 1);
            CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.Stool, ThingDefOf.WoodLog), plantPos, map, default, campAssetListRef);
            //var plant = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(ThingDefOf.PlantPot, ThingDefOf.WoodLog), plantPos, map, default, campAssetListRef) as Building_PlantGrower;

            //var realPlant = CampHelper.PrepAndGenerateThing(plant.GetPlantDefToGrow(), plant.Position, map, default, campAssetListRef, true) as Plant;
            //realPlant.Growth = 0f;
            //realPlant.sown = true;
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);
            var lover = Occupants.FirstOrDefault(col => col != null && CampHelper.ExistingColonistLovePartner(col) != null
               && Occupants.Contains(CampHelper.ExistingColonistLovePartner(col)));
            var otherLover = lover != null ? CampHelper.ExistingColonistLovePartner(lover) : null;
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && cell.z == CellRect.maxZ - 1).ToArray();

            for (int i = 0; i < cellSpots.Length; i++)
            {
                if (lover != null && i == 0) continue;
                else if (lover != null && i == 1)
                {
                    var dbThing = ThingMaker.MakeThing(CampDefOf.CAMakeshiftBedrollDouble, CampDefOf.CAMakeshiftTentLeather);
                    var doubleBed = GenSpawn.Spawn(dbThing, cellSpots[i], map, Rot4.South);
                    doubleBed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(doubleBed);
                    lover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    otherLover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                }
                else
                {
                    var thing = ThingMaker.MakeThing(CampDefOf.CAMakeshiftBedroll, CampDefOf.CAMakeshiftTentLeather);
                    var bed = GenSpawn.Spawn(thing, cellSpots[i], map, Rot4.South);
                    bed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(bed);
                    var pawnInNeedOfBed = Occupants.FirstOrDefault(occ => occ != null && occ != lover && occ != otherLover && (occ.ownership.OwnedBed == null || occ.ownership.OwnedBed.Map != map));
                    if (pawnInNeedOfBed != null) pawnInNeedOfBed.ownership.ClaimBedIfNonMedical((Building_Bed)bed);
                }
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var caheater = CampHelper.PrepAndGenerateThing(ThingDefOf.TorchLamp, caheaterPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(caheater, ModSettings.fuelStartingFillPercentage);

            var passiveCoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 1 && cell.z == CellRect.minZ + 1);
            var cooler = CampHelper.PrepAndGenerateThing(ThingDefOf.PassiveCooler, passiveCoolerPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(cooler, ModSettings.fuelStartingFillPercentage);
        }
    }
}
