using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips
{
    class MechChipModExt : DefModExtension
    {
        public string mechChipDefName = null;
        public bool hasShield = false;
        public List<HediffDef> mechChipDefs = new List<HediffDef>();
    }
}
