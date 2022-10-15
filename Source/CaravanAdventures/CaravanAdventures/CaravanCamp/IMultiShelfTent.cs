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
    public interface IMultiShelfTent
    {
        void FillShelfs(Map map, Caravan caravan);
        List<Building_Storage> GetShelfs();
    }
}
