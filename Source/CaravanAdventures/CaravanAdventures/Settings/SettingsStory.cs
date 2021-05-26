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

            //var viewRect = new Rect(0f, 0f, windowRect.width - 150, 1200f);
            //options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

            Text.Font = GameFont.Medium;
            options.Label("Story settings:".Colorize(Color.green), 40f);

            Text.Font = GameFont.Small;

            options.CheckboxLabeled("Apocalypse enabled", ref ModSettings.apocalypseEnabled);
            options.Label($"Apocalypse temperature change per day: {Math.Round(ModSettings.apocalypseTemperatureChangePerDay, 4)}  (Default: -0.084)");
            ModSettings.apocalypseTemperatureChangePerDay = options.Slider(ModSettings.apocalypseTemperatureChangePerDay, 0f, -0.5f);
            options.CheckboxLabeled("Apply Apocalypse temperature change while traveling the world map", ref ModSettings.allowApocToAlterTileTemp, "When enabled, the temperature change will apply to all tiles on the map. Vanilla itself applies those temperature changes from events only to maps and tiles that have a map loaded.");
            options.Gap();
            if (options.ButtonTextLabeled("Reset apocalypse event", "Reset")) CompCache.StoryWC.questCont.LastJudgment.ResetApocalypse(-0.1f);
            options.Gap();

            options.Label("Shrine mechanoid combat point multiplier:".Colorize(Color.green), 40f);
            options.Label("Multiplier: " + Math.Round(ModSettings.shrineMechDifficultyMultiplier, 2) + "  (default: 1.2)");
            ModSettings.shrineMechDifficultyMultiplier = options.Slider(ModSettings.shrineMechDifficultyMultiplier, 0.2f, 10f);
            options.Gap();
            options.Label("Shrine hunter assistance combat point multiplier:".Colorize(Color.green), 40f);
            options.Label("Multiplier: " + Math.Round(ModSettings.hunterAssistanceMult, 2) + "  (default: 1.2)");
            ModSettings.hunterAssistanceMult = options.Slider(ModSettings.hunterAssistanceMult, 0.2f, 2f);
            options.Gap();

            options.Label("Ancient shrine min/max spawn distance from the player's settlement:".Colorize(Color.green), 40f, "If the distance is too large for the world map, the distance will be halfed for the next attempt.");
            //options.Label("Multiplier: " + Math.Round(ModSettings.shrineDistance, 2) + "  (default: 1.2)");
            options.IntRange(ref ModSettings.shrineDistance, 50, 800);
            options.Gap();

            //options.Label("Max room combat points (x50 for huge shrine crypts):".Colorize(Color.red), 40f, "Only needed for crazy game setups with insane wealth.");
            //options.Label("Points: " + Math.Round(ModSettings.maxShrineCombatPoints, 0) + "  (default: 10000)");
            //ModSettings.maxShrineCombatPoints = options.Slider(ModSettings.maxShrineCombatPoints, 300f, 50000f);
            //options.Gap();
            options.CheckboxLabeled("Issue further shrine locations after story is complete", ref ModSettings.issueFurtherShrineLocationsAfterStoryEnd);
            options.CheckboxLabeled("Limit larger map sizes to tested medium-large map size for shrines", ref ModSettings.limitLargeMapSizesToTestedSize);

            options.Gap();
            options.CheckboxLabeled("Should sacrileg hunters be hostile towards the empire?", ref ModSettings.sacHuntersHostileTowardsEmpire);

            options.CheckboxLabeled("Remove royal title requirements", ref ModSettings.removeRoyalTitleRequirements);
            options.CheckboxLabeled("Only remove Acolyte and Knight title requirements", ref ModSettings.removeOnlyAcolyteAndKnightRoyalTitleRequirements);
            options.Gap();


            //options.EndScrollView(ref viewRect);
            options.End();

        }

    }
}
