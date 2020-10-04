using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CampArea
    {
        public IntVec3 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public CellRect CellRect { get; set; }
    }
}
