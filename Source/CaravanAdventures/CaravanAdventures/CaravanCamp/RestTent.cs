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
    public class RestTent : Tent
    {
        public List<Pawn> Occupants { get; set; }
        public bool SkipPawnAssignment { get; set; }

        public RestTent()
        {
            ForcedTentDirection = ForcedTentDirection.Horizontal;
            Occupants = new List<Pawn>();
            SupplyCost = ModSettings.campSupplyCostRestTent; // 3;
            SkipPawnAssignment = false;
        }

        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var lover = Occupants.FirstOrDefault(col => col != null && CampHelper.ExistingColonistLovePartner(col, Occupants) != null);
            var otherLover = lover != null ? CampHelper.ExistingColonistLovePartner(lover, Occupants) : null;
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && cell.z == CellRect.maxZ - 1).ToArray();
            var assigned = new List<Building_Bed>();
            var shareBed = lover != null
                && otherLover != null
                && BedUtility.WillingToShareBed(lover, otherLover)
                && !HealthAIUtility.ShouldSeekMedicalRest(lover)
                && !HealthAIUtility.ShouldSeekMedicalRest(otherLover);


            for (int i = 0; i < cellSpots.Length; i++)
            {
                if (lover != null && !lover.IsPrisoner && shareBed && i == 0) continue;
                else if (lover != null && !lover.IsPrisoner && shareBed && i == 1)
                {
                    var dbThing = ThingMaker.MakeThing(CampDefOf.CASpacerBedrollDouble, CampDefOf.CASpacerTentFabric);
                    var doubleBed = GenSpawn.Spawn(dbThing, cellSpots[i], map, Rot4.South);
                    doubleBed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(doubleBed);
                    if (SkipPawnAssignment) continue;
                    if (!CheckAssignableAndMarkBedForOwnerType(lover, doubleBed)
                        || !CheckAssignableAndMarkBedForOwnerType(otherLover, doubleBed)) continue;
                    lover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    otherLover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    assigned.Add((Building_Bed)doubleBed);
                }
                else
                {
                    var thing = ThingMaker.MakeThing(CampDefOf.CASpacerBedroll, CampDefOf.CASpacerTentFabric);
                    var bed = GenSpawn.Spawn(thing, cellSpots[i], map, Rot4.South);
                    bed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(bed);
                    if (SkipPawnAssignment) continue;
                    var pawnInNeedOfBed = Occupants.FirstOrDefault(occ => occ != null && (occ != lover && occ != otherLover || !shareBed) && !assigned.Any(x => x.OwnersForReading.Contains(occ)));
                    if (pawnInNeedOfBed != null)
                    {
                        if (!CheckAssignableAndMarkBedForOwnerType(pawnInNeedOfBed, bed)) continue;
                        pawnInNeedOfBed.ownership.ClaimBedIfNonMedical((Building_Bed)bed);
                        assigned.Add((Building_Bed)bed);
                    }
                }
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var caheater = CampHelper.PrepAndGenerateThing(ThingMaker.MakeThing(CampDefOf.CAAirConditioningHeater), caheaterPos, map, Rot4.South, campAssetListRef);
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
            var assigned = new List<Building_Bed>();
            var shareBed = lover != null
               && otherLover != null
               && BedUtility.WillingToShareBed(lover, otherLover)
               && !HealthAIUtility.ShouldSeekMedicalRest(lover)
               && !HealthAIUtility.ShouldSeekMedicalRest(otherLover);

            for (int i = 0; i < cellSpots.Length; i++)
            {
                if (lover != null && shareBed && i == 0) continue;
                else if (lover != null && shareBed && i == 1)
                {
                    var dbThing = ThingMaker.MakeThing(CampDefOf.CAMakeshiftBedrollDouble, CampDefOf.CAMakeshiftTentLeather);
                    var doubleBed = GenSpawn.Spawn(dbThing, cellSpots[i], map, Rot4.South);
                    doubleBed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(doubleBed);
                    if (SkipPawnAssignment) continue;
                    if (!CheckAssignableAndMarkBedForOwnerType(lover, doubleBed)
                        || !CheckAssignableAndMarkBedForOwnerType(otherLover, doubleBed)) continue;
                    lover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    otherLover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    assigned.Add((Building_Bed)doubleBed);
                }
                else
                {
                    var thing = ThingMaker.MakeThing(CampDefOf.CAMakeshiftBedroll, CampDefOf.CAMakeshiftTentLeather);
                    var bed = GenSpawn.Spawn(thing, cellSpots[i], map, Rot4.South);
                    bed.SetFaction(Faction.OfPlayer);
                    campAssetListRef.Add(bed);
                    if (SkipPawnAssignment) continue;
                    var pawnInNeedOfBed = Occupants.FirstOrDefault(occ => occ != null && (occ != lover && occ != otherLover || !shareBed) && !assigned.Any(x => x.OwnersForReading.Contains(occ)));
                    if (pawnInNeedOfBed != null)
                    {
                        if (!CheckAssignableAndMarkBedForOwnerType(pawnInNeedOfBed, bed)) continue;
                        pawnInNeedOfBed.ownership.ClaimBedIfNonMedical((Building_Bed)bed);
                        assigned.Add((Building_Bed)bed);
                    }
                }
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var caheater = CampHelper.PrepAndGenerateThing(ThingDefOf.TorchLamp, caheaterPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(caheater, ModSettings.fuelStartingFillPercentage);

            var passiveCoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.minX + 1 && cell.z == CellRect.minZ + 1);
            var cooler = CampHelper.PrepAndGenerateThing(ThingDefOf.PassiveCooler, passiveCoolerPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(cooler, ModSettings.fuelStartingFillPercentage);
        }

        protected virtual bool CheckAssignableAndMarkBedForOwnerType(Pawn pawn, Thing bed)
        {
            var bBed = bed as Building_Bed;
            if (bBed == null) return false;
            if (bBed.CompAssignableToPawn.IdeoligionForbids(pawn)) return false;
            if (pawn.IsPrisoner && !bBed.ForPrisoners) bBed.ForPrisoners = true;
            else if (pawn.IsSlave && !bBed.ForSlaves) bBed.GetType().GetField("forOwnerType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(bBed, BedOwnerType.Slave);
            return true;
        }

        protected virtual void CheckAndPostApplyBedState(Map map, BedOwnerType ownerType)
        {
            var beds = CellRect.Cells.Select(cell => cell.GetFirstThing<Building_Bed>(map));
            foreach (var bed in beds)
            {
                if (bed == null || (ownerType == BedOwnerType.Prisoner ? bed.ForPrisoners : bed.ForSlaves)) continue;
                var bedFieldInstance = bed.GetType().GetField("forOwnerType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (bedFieldInstance == null) continue;
                bedFieldInstance.SetValue(bed, ownerType);

                var district = bed.GetDistrict(RegionType.Set_Passable);
                if (district == null) continue;
                district.Notify_RoomShapeOrContainedBedsChanged();
                var room = district.Room;
                if (room != null) room.Notify_RoomShapeChanged();
            }
        }
    }
}
