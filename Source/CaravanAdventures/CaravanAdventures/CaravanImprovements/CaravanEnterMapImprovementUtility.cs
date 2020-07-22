using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CaravanAdventures.CaravanImprovements
{
    public static class CaravanEnterMapImprovementUtility
    {
        private static List<Pawn> tmpPawns = new List<Pawn>();

        public static void Enter(Caravan caravan, Map map, CaravanEnterMode enterMode, CaravanDropInventoryMode dropInventoryMode = CaravanDropInventoryMode.DoNotDrop, bool draftColonists = false, Predicate<IntVec3> extraCellValidator = null)
        {
            if (enterMode == CaravanEnterMode.None)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Caravan ",
                    caravan,
                    " tried to enter map ",
                    map,
                    " with enter mode ",
                    enterMode
                }), false);
                enterMode = CaravanEnterMode.Edge;
            }
            IntVec3 enterCell = GetEnterCell(caravan, map, enterMode, extraCellValidator);
            Func<Pawn, IntVec3> spawnCellGetter = (Pawn p) => CellFinder.RandomSpawnCellForPawnNear(enterCell, map, 4);
            Enter(caravan, map, spawnCellGetter, dropInventoryMode, draftColonists);
        }

        public static void Enter(Caravan caravan, Map map, Func<Pawn, IntVec3> spawnCellGetter, CaravanDropInventoryMode dropInventoryMode = CaravanDropInventoryMode.DoNotDrop, bool draftColonists = false)
        {
            tmpPawns.Clear();
            tmpPawns.AddRange(caravan.PawnsListForReading);
            for (int i = 0; i < tmpPawns.Count; i++)
            {
                IntVec3 loc = spawnCellGetter(tmpPawns[i]);
                GenSpawn.Spawn(tmpPawns[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
            }
            if (dropInventoryMode == CaravanDropInventoryMode.DropInstantly)
            {
                DropAllInventory(tmpPawns);
            }
            else if (dropInventoryMode == CaravanDropInventoryMode.UnloadIndividually)
            {
                for (int j = 0; j < tmpPawns.Count; j++)
                {
                    tmpPawns[j].inventory.UnloadEverything = true;
                }
            }
            // todo add gizmo to allow unloading later

            if (draftColonists)
            {
                DraftColonists(tmpPawns);
            }
            if (map.IsPlayerHome)
            {
                for (int k = 0; k < tmpPawns.Count; k++)
                {
                    if (tmpPawns[k].IsPrisoner)
                    {
                        tmpPawns[k].guest.WaitInsteadOfEscapingForDefaultTicks();
                    }
                }
            }

            Find.TickManager.Pause();
            //caravan.RemoveAllPawns();
            //if (!caravan.Destroyed)
            //{
            //    caravan.Destroy();
            //}
            tmpPawns.Clear();
        }

        private static IntVec3 GetEnterCell(Caravan caravan, Map map, CaravanEnterMode enterMode, Predicate<IntVec3> extraCellValidator)
        {
            if (enterMode == CaravanEnterMode.Edge)
            {
                return FindNearEdgeCell(map, extraCellValidator);
            }
            if (enterMode != CaravanEnterMode.Center)
            {
                throw new NotImplementedException("CaravanEnterMode");
            }
            return FindCenterCell(map, extraCellValidator);
        }

        private static IntVec3 FindNearEdgeCell(Map map, Predicate<IntVec3> extraCellValidator)
        {
            Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map) && !x.Fogged(map);
            Faction hostFaction = map.ParentFaction;
            IntVec3 root;
            if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => baseValidator(x) && (extraCellValidator == null || extraCellValidator(x)) && ((hostFaction != null && map.reachability.CanReachFactionBase(x, hostFaction)) || (hostFaction == null && map.reachability.CanReachBiggestMapEdgeRoom(x))), map, CellFinder.EdgeRoadChance_Neutral, out root))
            {
                return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
            }
            if (extraCellValidator != null && CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => baseValidator(x) && extraCellValidator(x), map, CellFinder.EdgeRoadChance_Neutral, out root))
            {
                return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
            }
            if (CellFinder.TryFindRandomEdgeCellWith(baseValidator, map, CellFinder.EdgeRoadChance_Neutral, out root))
            {
                return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
            }
            Log.Warning("Could not find any valid edge cell.", false);
            return CellFinder.RandomCell(map);
        }

        private static IntVec3 FindCenterCell(Map map, Predicate<IntVec3> extraCellValidator)
        {
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false);
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

        public static void DropAllInventory(List<Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                pawns[i].inventory.DropAllNearPawn(pawns[i].Position, false, true);
            }
        }

        private static void DraftColonists(List<Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].IsColonist)
                {
                    pawns[i].drafter.Drafted = true;
                }
            }
        }

        private static bool TryRandomNonOccupiedClosewalkCellNear(IntVec3 root, Map map, int radius, out IntVec3 result)
        {
            return CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(map) && c.GetFirstPawn(map) == null, null, out result, 999999);
        }


    }
}
