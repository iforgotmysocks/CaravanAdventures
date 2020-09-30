using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_LastJudgment : IExposable
    {

        private GameCondition_Apocalypse apocalypse;

        public QuestCont_LastJudgment()
        {
        
        }

        public void StartApocalypse(float minTemp, float annualIncrease)
        {
            apocalypse = apocalypse ?? new GameCondition_Apocalypse();
            apocalypse.TempOffset = minTemp;
            apocalypse.Permanent = true;
            apocalypse.startTick = Find.TickManager.TicksGame;
            apocalypse.AnualIncrease = annualIncrease;
        }


        public void ExposeData()
        {
            Scribe_References.Look(ref apocalypse, "apocalypse");
        }

        internal void CreateLastJudgmentMP(ref LastJudgmentMP lastJudgmentMP, int tile)
        {
			lastJudgmentMP = (LastJudgmentMP)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.CALastJudgmentMP);
			lastJudgmentMP.SetFaction(StoryUtility.EnsureSacrilegHunters());
			lastJudgmentMP.Tile = tile;
			Find.WorldObjects.Add(lastJudgmentMP);
			MapGenerator.GenerateMap(new IntVec3(50, 1, 50), lastJudgmentMP, lastJudgmentMP.MapGeneratorDef, lastJudgmentMP.ExtraGenStepDefs, null);

			var stateBackup = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;

			var map = lastJudgmentMP.Map;

			foreach (var cell in map.AllCells)
			{
				Log.Message($"{cell} {cell.y}");
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
				map.terrainGrid.SetUnderTerrain(cell, TerrainDefOf.Gravel);
			}

			for (int i = 0; i < map.Size.x; i++)
			{
				for (int j = 0; j < map.Size.z; j++)
				{
					var currentCell = new IntVec3(i, 0, j);
					if ((i == 4 || i == map.Size.x - 5) && (j % 10 == 0 && j > 0)
					|| (j == 4 || j == map.Size.z - 5) && (i % 10 == 0 && i > 0)) GenSpawn.Spawn(ThingDef.Named("CARedLight"), currentCell, map);

					if (i == 0 || i == map.Size.x - 1 || j == 0 || j == map.Size.z - 1)
					{
						var wall = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel);
						GenSpawn.Spawn(wall, currentCell, map);
					}
				}
			}

			foreach (var cell in map.AllCells)
			{
				map.fogGrid.Unfog(cell);
				map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
			}

			//map.terrainGrid.Drawer.SetDirty();
			//map.terrainGrid.Drawer.CellBoolDrawerUpdate();

			Current.ProgramState = stateBackup; 
			
		}
    }
}
