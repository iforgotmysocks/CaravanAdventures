using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public interface IZoneTent
    {
        Zone GetZone();
        void CreateZone(Map map);
        void ApplyInventory(Map map, Caravan caravan);
    }
}
