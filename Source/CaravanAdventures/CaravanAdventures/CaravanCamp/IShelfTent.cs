using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public interface IShelfTent
    {
        void FillShelfs(Map map, Caravan caravan);
        Building_Storage GetShelf();
    }
}
