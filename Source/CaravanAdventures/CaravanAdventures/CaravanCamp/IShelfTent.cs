using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public interface IShelfTent
    {
        void FillShelfs(Map map, Caravan caravan);
        Building_Storage GetShelf();
    }
}
