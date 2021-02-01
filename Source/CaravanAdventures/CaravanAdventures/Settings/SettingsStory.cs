using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Settings
{
    class SettingsStory : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsStory()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = Vector2.zero;

            width = 600f;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - InitialSize.y) / 2f, width, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            var viewRect = new Rect(0f, 0f, windowRect.width - 150, 1200f);
            options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

            Text.Font = GameFont.Medium;
            options.Label("Story settings:".Colorize(Color.red), 40f);

            Text.Font = GameFont.Small;

            options.CheckboxLabeled("Apocalypse enabled", ref ModSettings.apocalypseEnabled);
            options.Label($"Apocalypse temperature change per day: {Math.Round(ModSettings.apocalypseTemperatureChangePerDay, 4)}  (Default: -0.084)");
            ModSettings.apocalypseTemperatureChangePerDay = options.Slider(ModSettings.apocalypseTemperatureChangePerDay, 0f, -0.5f);
            if (options.ButtonTextLabeled("Reset apocalypse event", "Reset")) CompCache.StoryWC.questCont.LastJudgment.ResetApocalypse(-0.1f);
            options.Gap();

            options.Label("Shrine mech combat point multiplier:".Colorize(Color.red), 40f);
            options.Label("Multiplier: " + Math.Round(ModSettings.shrineMechDifficultyMultiplier, 2) + "  (default: 1.2)");
            ModSettings.shrineMechDifficultyMultiplier = options.Slider(ModSettings.shrineMechDifficultyMultiplier, 0.2f, 10f);
            options.Gap();

            options.CheckboxLabeled("Remove royal title requirements", ref ModSettings.removeRoyalTitleRequirements);
            options.Gap();
            options.CheckboxLabeled("Only remove Acolyte and Knight title requirements", ref ModSettings.removeOnlyAcolyteAndKnightRoyalTitleRequirements);
            options.Gap();


            options.EndScrollView(ref viewRect);
            options.End();

        }

    }
}
