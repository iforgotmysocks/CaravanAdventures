using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CampBuilder
    {
        public static bool GenerateCamp(Caravan caravan, Map map)
        {
			// todos
			// Find fabric and leather used to build buildings
			// create rect large enough to fit tents for x pawns
			// build bridges on non compatible ground
			// generate buildings 


			/*
             Sketch sketch = SketchGen.Generate(SketchResolverDefOf.Monument, resolveParams);
			sketch.Spawn(map, rp.rect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, true, true, null, false, true, null, null);
			CellRect rect = SketchGenUtility.FindBiggestRect(sketch, delegate(IntVec3 x)
			{
				if (sketch.TerrainAt(x) != null)
				{
					return !sketch.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall);
				}
				return false;
			}).MovedBy(rp.rect.CenterCell);
			for (int i = 0; i < sketch.Things.Count; i++)
			{
				if (sketch.Things[i].def == ThingDefOf.Wall)
				{
					IntVec3 intVec = sketch.Things[i].pos + rp.rect.CenterCell;
					if (cellRect.IsEmpty)
					{
						cellRect = CellRect.SingleCell(intVec);
					}
					else
					{
						cellRect = CellRect.FromLimits(Mathf.Min(cellRect.minX, intVec.x), Mathf.Min(cellRect.minZ, intVec.z), Mathf.Max(cellRect.maxX, intVec.x), Mathf.Max(cellRect.maxZ, intVec.z));
					}
				}
			} 
              
              
            */

			return true;
        }
    }
}
