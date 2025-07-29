using Verse;
using CaravanAdventures.Patches;

namespace CaravanAdventures
{
    public class Main : Mod
    {
        internal static ModSettings Settings { get; set; }

        public Main(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ModSettings>();
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
