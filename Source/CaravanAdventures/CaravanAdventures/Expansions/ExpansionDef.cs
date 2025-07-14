using Verse;

namespace CaravanAdventures.Expansions
{
    class ExpansionDef : Def
    {
#pragma warning disable CS0649
        public string expansionName;
        public string assemblyName;
        public bool replacesContent;
        public string langKeyPrefix;
        public Settings.ExpSettingsDef expSettingsDef;
    }
}
