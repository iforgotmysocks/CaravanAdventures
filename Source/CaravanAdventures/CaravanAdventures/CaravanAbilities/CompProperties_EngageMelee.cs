using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    internal class CompProperties_EngageMelee : CompProperties
    {
        public int interval = 30;

        public CompProperties_EngageMelee()
        {
            this.compClass = typeof(CompEngageMelee);
        }
    }
}
