using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    interface IZoneTent
    {
        void CreateZone(Map map);

        void ApplyInventory(Map map, Caravan caravan);
    }
}
