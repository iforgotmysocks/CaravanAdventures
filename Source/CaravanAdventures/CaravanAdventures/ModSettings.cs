using CaravanAdventures.CaravanStory;
using CaravanAdventures.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures
{
    class ModSettings : Verse.ModSettings
    {
        // not saved!
        public static Rect lastRect = default;
        public bool toggleTest = false;


        // general
        public static bool debug = false;
        public static bool debugMessages = false;

        // improvements
        public static bool removeRoyalTitleRequirements = true;
        public static bool removeOnlyAcolyteAndKnightRoyalTitleRequirements = true;

        // camp
        public static bool generateStorageForAllInventory = true;
        public static bool letAnimalsRunFree = false;

        // abilities
        // - ancient gift
        public static float ancientGiftPassivePsyfocusGainPerSec = 0.00035f;
        // - thunderbolt
        public static float mechanoidDissmemberChance = 0.6f;
        public static float humanDissmemberChance = 0.3f;
        public static float additionalBuildingAreaDamageMin = 0.5f;
        public static float additionalBuildingAreaDamageMax = 0.75f;
        // - meditation
        public static float psyfocusToRestore = 0.2f;
        public static float plantScoreMultiplier = 2.0f;
        // - protective aura
        public static float healingPerSecond = 0.05f;
        public static bool onlyHealPermWhenGifted = false;
        public static bool stopMentalBreaks = false;
        public static int maxLinkedAuraPawns = 4;
        // - light
        public static float lightDuration = 1200f;

        // story
        // todo remove the ability to disable the apoc? Can always set the increase to 0
        public static bool storyEnabled = true;
        public static bool limitLargeMapSizesToTestedSize = true;
        public static bool apocalypseEnabled = true;
        public static float apocalypseTemperatureChangePerDay = -0.084f;
        public static bool issueFurtherShrineLocationsAfterStoryEnd = true;

        // shrines
        public static float shrineMechDifficultyMultiplier = 1.2f;

        // bounty
        public static float envoyDurationTimeForBountyRelationHagglingInDays = 1f;
        public static float itemRestockDurationInDays = 1f;
        public static float alliedAssistanceDurationInDays = 1f;
        public static float veteranResetTimeInDays = 3f;
        public static bool allowBountyFromBuildingInstigators = true;
        public static bool allowBuyingBountyWithSilver = true;
        public static float bountyValueMult = 0.25f;

        //public static ModSettings Get() => LoadedModManager.GetMod<CaravanAdventures.Main>().GetSettings<ModSettings>();
        private Vector2 scrollPos = Vector2.zero;

        public override void ExposeData()
        {
            base.ExposeData();
            // debug
            Scribe_Values.Look(ref debug, "debug", false);
            Scribe_Values.Look(ref debugMessages, "debugMessages", false);

            // improvements
            Scribe_Values.Look(ref removeRoyalTitleRequirements, "removeRoyalTitleRequirements", true);
            Scribe_Values.Look(ref removeOnlyAcolyteAndKnightRoyalTitleRequirements, "removeOnlyAcolyteAndKnightRoyalTitleRequirements", true);

            // camp
            Scribe_Values.Look(ref generateStorageForAllInventory, "generateStorageForAllInventory", false);
            Scribe_Values.Look(ref letAnimalsRunFree, "letAnimalsRunFree", false);

            // abilities
            // - ancient gift
            Scribe_Values.Look(ref ancientGiftPassivePsyfocusGainPerSec, "ancientGiftPassivePsyfocusGainPerSec", 0.00035f);
            // - thunderbolt
            Scribe_Values.Look(ref mechanoidDissmemberChance, "mechanoidDissmemberChance", 0.6f);
            Scribe_Values.Look(ref humanDissmemberChance, "humanDissmemberChance", 0.3f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMin, "additionalBuildingAreaDamageMin", 0.5f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMax, "additionalBuildingAreaDamageMax", 0.75f);
            // - meditation
            Scribe_Values.Look(ref psyfocusToRestore, "psyfocusToRestore", 0.2f);
            Scribe_Values.Look(ref plantScoreMultiplier, "plantScoreMultiplier", 2.0f);
            // - protective aura
            Scribe_Values.Look(ref healingPerSecond, "healingPerSecond", 0.05f);
            Scribe_Values.Look(ref stopMentalBreaks, "stopMentalBreaks", false);
            Scribe_Values.Look(ref onlyHealPermWhenGifted, "onlyHealPermWhenGifted", false);
            Scribe_Values.Look(ref maxLinkedAuraPawns, "maxLinkedAuraPawns", 4);
            // - light
            Scribe_Values.Look(ref lightDuration, "lightDuration", 1200f);

            // story
            Scribe_Values.Look(ref storyEnabled, "storyEnabled", true);
            Scribe_Values.Look(ref limitLargeMapSizesToTestedSize, "limitLargeMapSizesToTestedSize", true);
            Scribe_Values.Look(ref apocalypseEnabled, "apocalypseEndabled");
            Scribe_Values.Look(ref apocalypseTemperatureChangePerDay, "apocalypseTemperatureChangePerDay", -0.084f);
            Scribe_Values.Look(ref issueFurtherShrineLocationsAfterStoryEnd, "issueFurtherShrineLocationsAfterStoryEnd", true);

            // shrines
            Scribe_Values.Look(ref shrineMechDifficultyMultiplier, "shrineMechDifficultyMultiplier", 1.2f);

            // bounty
            Scribe_Values.Look(ref envoyDurationTimeForBountyRelationHagglingInDays, "envoyDurationTimeForBountyRelationHagglingInDays", 1f);
            Scribe_Values.Look(ref itemRestockDurationInDays, "itemRestockDurationInDays", 1f);
            Scribe_Values.Look(ref alliedAssistanceDurationInDays, "alliedAssistanceDurationInDays", 1f);
            Scribe_Values.Look(ref veteranResetTimeInDays, "veteranResetTimeInDays", 3f);
            Scribe_Values.Look(ref allowBountyFromBuildingInstigators, "allowBountyFromBuildingInstigators", true);
            Scribe_Values.Look(ref allowBuyingBountyWithSilver, "allowBuyingBountyWithSilver", true);
            Scribe_Values.Look(ref bountyValueMult, "bountyValueMult", 0.25f);
        }

        public static Rect BRect(Rect rect)
        {
            lastRect = rect;
            return rect;
        }
        public static Rect BRect(float x, float y, float width, float height)
        {
            lastRect = new Rect(x, y, width, height);
            return lastRect;
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);
            //GUI.BeginGroup(wrect);
            //Widgets.BeginScrollView(wrect, ref this.scrollPos, new Rect(0f, 0f, wrect.width, 700f));

            var viewRect = new Rect(0f, 0f, wrect.width, 1600);
            options.BeginScrollView(wrect, ref this.scrollPos, ref viewRect);

            var debugRect = BRect(options.GetRect(Text.LineHeight));
            //options.CheckboxLabeled("Debug mode", ref debug);
            //options.CheckboxLabeled("Debug messages", ref debugMessages);

            //if (options.ButtonText("Reset final shrine flags")) StoryUtility.ResetLastShrineFlags();
            //if (options.ButtonText("Print world pawns")) Helper.PrintWorldPawns();
            //if (options.ButtonText("Reset full story")) StoryUtility.RestartStory();

            Widgets.CheckboxLabeled(BRect(0, lastRect.y, options.ColumnWidth / 5 - 20, lastRect.height), "Debug mode", ref debug);
            Widgets.CheckboxLabeled(BRect(options.ColumnWidth / 5 * 1, lastRect.y, options.ColumnWidth / 5 - 20, lastRect.height), "Debug messages", ref debugMessages);
            if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 2, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Reset final shrine flags")) StoryUtility.ResetLastShrineFlags();
            if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 3, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Print world pawns")) Helper.PrintWorldPawns();
            if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 4, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Reset full story")) StoryUtility.RestartStory();


            options.GapLine();

            Text.Font = GameFont.Medium;
            var cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Filter settings".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref toggleTest);
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsFilters());
            options.Gap(10);
            options.Label($"Adjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();
            options.Gap();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Camp settings".HtmlFormatting("00ff00", false, 20));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref toggleTest);
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsCamp());
            options.Gap(10);
            options.Label($"Adjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();
            options.Gap();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Story settings".HtmlFormatting("00ff00", false, 20));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref toggleTest);
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsStory());
            options.Gap(10);
            options.Label($"Adjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();
            options.Gap();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Ancient abilitiy settings".HtmlFormatting("00ff00", false, 20));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref toggleTest);
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsAbilities());
            options.Gap(10);
            options.Label($"Adjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();
            options.Gap();


            // move to improvement settings -> or rather delete and adjust that via def patch
            options.Label("Settlement price adjustments");
            options.Label("Multiplies the current money for settlements with less then 3k silver by 3, and those with more by 2");
            if (options.ButtonTextLabeled("Increase Settlement money", "Increase")) Helper.AdjustSettlementPrices();
            // todo add min and max multiplier used in the def patch for settlement money
            options.Gap();

            // camp settings
            options.CheckboxLabeled("Generate storage tents for all items and unload", ref generateStorageForAllInventory);
            options.CheckboxLabeled("Let animals run free", ref letAnimalsRunFree);

            options.EndScrollView(ref viewRect);
            //Widgets.EndScrollView();
            //GUI.EndGroup();
            options.End();
            this.Write();
        }




    }
}
