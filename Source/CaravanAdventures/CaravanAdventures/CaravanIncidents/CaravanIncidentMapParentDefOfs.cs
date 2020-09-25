using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

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
