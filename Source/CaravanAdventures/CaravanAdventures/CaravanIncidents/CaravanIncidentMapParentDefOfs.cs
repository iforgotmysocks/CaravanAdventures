using RimWorld;

namespace CaravanAdventures.CaravanIncidents
{
    [DefOf]
    public static class CaravanIncidentMapParentDefOfs
    {
        static CaravanIncidentMapParentDefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CaravanIncidentMapParentDefOfs));
        }

        public static WorldObjectDef CADamselInDistressMapParent;
    }
}
