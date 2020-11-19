using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public enum ForcedTentDirection { Horizontal, Vertical, None }
    abstract class CampArea
    {
        public int CoordSize { get; set; }
        public List<IntVec3> Coords { get; set; }
        public IntVec3 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public CellRect CellRect { get; set; }
        public ForcedTentDirection ForcedTentDirection { get; set; }
        public int SupplyCost { get; set; }

        public CampArea()
        {
            Width = 5;
            Height = 5;
            CoordSize = 1;
            Coords = new List<IntVec3>();
            ForcedTentDirection = ForcedTentDirection.None;
            SupplyCost = 0;
        }

        public abstract void Build(Map map, List<Thing> campAssetListRef);

        public abstract void BuildTribal(Map map, List<Thing> campAssetListRef);
    }
}
