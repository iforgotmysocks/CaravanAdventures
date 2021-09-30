using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.Expansions
{
    class ExpansionManager
    {
        public static ExpansionDef ActiveExpansion => CompatibilityPatches.RMInst ? Expansions.ExpansionDefOf.ExpRimedieval : null;
    }
}
