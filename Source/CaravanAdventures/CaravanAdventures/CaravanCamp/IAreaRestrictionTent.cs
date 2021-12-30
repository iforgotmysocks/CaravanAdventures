using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    public interface IAreaRestrictionTent
    {
        void CreateNewRestrictionArea(Map map, Caravan caravan);
        void AssignPawnsToAreas(Map map, Caravan caravan);
    }
}