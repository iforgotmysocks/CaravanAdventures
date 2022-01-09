using Verse;
using CaravanAdventures.Patches;

namespace CaravanAdventures
{
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings>();
            HarmonyPatcher.RunEarlyPatches();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<ModSettings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Caravan Adventures";
        }
    }
}
