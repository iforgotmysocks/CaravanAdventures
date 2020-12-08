using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CaravanAdventures.CaravanCamp
{
    class CampHelper
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

        internal static Thing PrepAndGenerateThing(object objThing, IntVec3 cell, Map map, Rot4 rot, List<Thing> campAssetListRef, bool skipFaction = false)
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

        public static Thing GetFirstOrderedThingOfCategoryFromCaravan(Caravan caravan, ThingCategoryDef[] validCategories, ThingDef[] unvalidThings = null)
        {
            Thing selected = null;
            foreach (var category in validCategories)
            {
                selected = caravan.AllThings.Where(x => x?.def?.thingCategories != null 
                    && x.def.thingCategories.Contains(category) 
                    && ((unvalidThings == null || unvalidThings.Length == 0) || Array.IndexOf(unvalidThings, x.def) == -1))
                    .OrderByDescending(x => x.stackCount).FirstOrDefault();

                if (selected != null) return selected;
            }
            return selected;
        }

        internal static void BringToNormalizedTemp(CellRect cellRect, Map map)
        {
            var roomGroup = cellRect.CenterCell.GetRoomGroup(map);
            var remainingTemp = 22 - roomGroup.Temperature;
            roomGroup.PushHeat(remainingTemp * cellRect.Cells.Where(curCell => !cellRect.EdgeCells.Contains(curCell)).Count());
        }

        internal static void AddAnimalFreeAreaRestriction(IEnumerable<IZoneTent> parts, Map map, Caravan caravan, bool assignAnimals = false)
        {
            var animalArea = new Area_Allowed(map.areaManager);
            map.areaManager.AllAreas.Add(animalArea);
            animalArea.SetLabel("CAAnimalNoFoodAreaLabel".Translate());
            map.AllCells.ToList().ForEach(cell => animalArea[cell] = true);
            parts.Where(part => part.GetZone() != null).Select(part => part.GetZone()).ToList().ForEach(zone => zone.Cells.ForEach(cell => animalArea[cell] = false));
            animalArea.AreaUpdate();

            if (assignAnimals) foreach (var animal in caravan.PawnsListForReading.Where(pawn => pawn.RaceProps.Animal)) animal.playerSettings.AreaRestriction = animalArea;
        }

        internal static void AssignQualityReflectiveOfSkill(Building thing, int skillLevel, int lowestSkill = 6)
        {
            var compQual = thing.TryGetComp<CompQuality>();
            if (compQual == null) return;
            skillLevel = lowestSkill > skillLevel ? lowestSkill : skillLevel;
            compQual.SetQuality(QualityUtility.GenerateQualityCreatedByPawn(skillLevel, false), ArtGenerationContext.Colony);
        }
    }
}
