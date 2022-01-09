using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CaravanAdventures.CaravanCamp
{
    public class CampHelper
    {
        public static IntVec3 FindCenterCell(Map map, Predicate<IntVec3> extraCellValidator)
        {
            var traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false);
            Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParms);
            IntVec3 result;
            if (extraCellValidator != null && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => baseValidator(x) && extraCellValidator(x), map, out result))
            {
                return result;
            }
            if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(baseValidator, map, out result))
            {
                return result;
            }
            Log.Warning("Could not find any valid cell.", false);
            return CellFinder.RandomCell(map);
        }

        public static Thing PrepAndGenerateThing(object objThing, IntVec3 cell, Map map, Rot4 rot, List<Thing> campAssetListRef, bool skipFaction = false)
        {
            var spawnedThing = objThing is Thing thing ? GenSpawn.Spawn(thing, cell, map, rot) 
                : objThing is ThingDef thingDef ? GenSpawn.Spawn(thingDef, cell, map) : null;

            if (spawnedThing == null)
            {
                Log.Warning($"Failed to spawn thing that should have been spawned in camp setup");
                return null;
            }
            if (!skipFaction) spawnedThing.SetFaction(Faction.OfPlayer);
            campAssetListRef.Add(spawnedThing);
            return spawnedThing;
        }

        public static IEnumerable<Thing> GetOrderedThingsOfCategoryFromCaravan(Caravan caravan, ThingCategoryDef[] validCategories, ThingDef[] unvalidThings = null)
            => caravan.AllThings.Where(x => x?.def?.thingCategories != null
                    && validCategories.Any(cat => x.def.thingCategories.Contains(cat))
                    && ((unvalidThings == null || unvalidThings.Length == 0) || Array.IndexOf(unvalidThings, x.def) == -1))
                    .OrderByDescending(x => x.MarketValue).ThenByDescending(x => x.stackCount);
        

        public static Thing GetFirstOrderedThingOfCategoryFromCaravan(Caravan caravan, ThingCategoryDef[] validCategories, ThingDef[] unvalidThings = null)
            => GetOrderedThingsOfCategoryFromCaravan(caravan, validCategories, unvalidThings)?.FirstOrDefault();

        public static void BringToNormalizedTemp(CellRect cellRect, Map map)
        {
            var roomGroup = cellRect.CenterCell.GetRoomGroup(map);
            var remainingTemp = 22 - roomGroup.Temperature;
            roomGroup.PushHeat(remainingTemp * cellRect.Cells.Where(curCell => !cellRect.EdgeCells.Contains(curCell)).Count());
        }

        public static void AddAnimalFreeAreaRestriction(IEnumerable<CampArea> parts, Map map, Caravan caravan, bool assignAnimals = false)
        {
            var animalArea = new Area_Allowed(map.areaManager);
            map.areaManager.AllAreas.Add(animalArea);
            animalArea.SetLabel("CAAnimalNoFoodAreaLabel".Translate());
            map.AllCells.ToList().ForEach(cell => animalArea[cell] = true);
            parts.OfType<IZoneTent>().Where(part => part.GetZone() != null).Select(part => part.GetZone()).ToList().ForEach(zone => zone.Cells.ForEach(cell => animalArea[cell] = false));
            parts.OfType<PlantTent>().ToList().ForEach(tent => tent.CellRect.Cells.Where(cell => !tent.CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => animalArea[cell] = false));
            animalArea.AreaUpdate();

            if (assignAnimals) foreach (var animal in caravan.PawnsListForReading.Where(pawn => pawn.RaceProps.Animal)) animal.playerSettings.AreaRestriction = animalArea;
        }

        public static void AssignQualityReflectiveOfSkill(Building thing, int skillLevel, int lowestSkill = 6)
        {
            var compQual = thing.TryGetComp<CompQuality>();
            if (compQual == null) return;
            skillLevel = lowestSkill > skillLevel ? lowestSkill : skillLevel;
            compQual.SetQuality(QualityUtility.GenerateQualityCreatedByPawn(skillLevel, false), ArtGenerationContext.Colony);
        }

        public static Pawn ExistingColonistLovePartner(Pawn pawn, List<Pawn> group = null)
        {
            Predicate<Pawn> predicate = null;
            if (group == null) predicate = x => x.IsColonist;
            else predicate = x => group.Contains(x);

            var firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, predicate);
            if (firstDirectRelationPawn != null) return firstDirectRelationPawn;
            firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, predicate);
            if (firstDirectRelationPawn != null) return firstDirectRelationPawn;
            firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, predicate);
            if (firstDirectRelationPawn != null) return firstDirectRelationPawn;
            return null;
        }

        public static void RefuelByPerc(Thing consumer, int fuelPerc = 100)
        {
            var refuelComp = consumer.TryGetComp<CompRefuelable>();
            if (refuelComp == null) return;
            if (fuelPerc == -1) fuelPerc = ModSettings.fuelStartingFillPercentage;
            if (fuelPerc != 100) refuelComp.ConsumeFuel(refuelComp.Fuel);
            refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel() * fuelPerc / 100f);
        }
    }
}
