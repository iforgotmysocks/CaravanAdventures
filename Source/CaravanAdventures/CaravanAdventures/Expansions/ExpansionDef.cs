using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Expansions
{
    class ExpansionDef : Def
    {
#pragma warning disable CS0649
        public string expansionName;
        public string assemblyName;
        public bool changesText;
        public string langKeyPrefix;
        public Settings.ExpSettingsDef expSettingsDef;
    }
}
