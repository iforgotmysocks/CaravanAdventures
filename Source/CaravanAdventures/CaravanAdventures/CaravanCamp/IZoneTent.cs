using RimWorld.Planet;
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
