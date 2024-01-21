using CaravanAdventures.CaravanStory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Patches.Compatibility
{
    internal class Rimedieval
    {
        public static bool MechsDisabled { get; private set; }

        public static bool CheckRimedievalMechsDisabled(Assembly assembly)
        {
            var rimedievalSettingsType = assembly.GetType("Rimedieval.RimedievalSettings");
            var rmObject = LoadedModManager.GetMod(assembly.GetType("Rimedieval.RimedievalMod"));
            var getSettingsInfo = typeof(Mod).GetMethod("GetSettings").MakeGenericMethod(rimedievalSettingsType).Invoke(rmObject, new object[] {} );
            if (!bool.TryParse(getSettingsInfo.GetType().GetField("disableMechanoids", BindingFlags.Public | BindingFlags.Instance).GetValue(getSettingsInfo).ToString(), out var disabledMechanoidsResult)) return false;
            MechsDisabled = disabledMechanoidsResult;
            return disabledMechanoidsResult;
        }

        public static void RemoveSacHunterTechHediffs()
        {
            foreach (var def in DefDatabase<PawnKindDef>.AllDefs.Where(x => x.defName.StartsWith("CASacrilegHunters")))
            {
                def.techHediffsChance = 0f;
                def.techHediffsRequired = new List<ThingDef>();
            }
        }
    }
}
