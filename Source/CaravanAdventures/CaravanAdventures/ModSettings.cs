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

        // general
        public static bool debug = false;
        public static bool debugMessages = false;

        // improvements
        public static bool removeRoyalTitleRequirements = true;
        public static bool removeOnlyAcolyteAndKnightRoyalTitleRequirements = true;

        // camp
        public static bool generateStorageForAllInventory = true;
        public static bool letAnimalsRunFree = false;
        public static bool hasProductionTent = true;
        public static bool hasStorageTent = true;
        public static bool hasMedicalTent = true;
        public static bool hasAnimalArea = true;
        public static bool hasPrisonTent = true;
        public static bool hasPlantTent = true;
        public static bool hasSupplyCostsDisabled = false;
        public static bool autoApplyCampGearRecipes = true;
        public static bool autoApplyCampClearSnowArea = false;
        public static bool useCustomMapSize = false;
        public static IntVec3 campMapSize = new IntVec3(250, 1, 250);
        public static int maxCampSupplyCost = 75;
        public static int fuelStartingFillPercentage = 50;
        public static bool showSupplyCostsInGizmo = true;
        public static bool decorativeFencePosts = true;
        public static bool useAnimalOnlyFoodForAnimalArea = false;

        // camp cost TODO -> create defs for tent types and move it there
        public static int campSupplyCostAnimalArea = 1;
        public static int campSupplyCostCampCenter = 1;
        public static int campSupplyCostFoodTent = 3;
        public static int campSupplyCostMedicalTent = 4;
        public static int campSupplyCostPlantTent = 6;
        public static int campSupplyCostProductionTent = 4;
        public static int campSupplyCostRestTent = 3;
        public static int campSupplyCostStorageTent = 3;

        // camp hardcoded for now (plsdonthateme)
        public static IntVec3 tentSize = new IntVec3(5, 0, 5);

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
        public static bool sacHuntersHostileTowardsEmpire = false;
        public static bool allowApocToAlterTileTemp = false;

        // shrines
        public static float shrineMechDifficultyMultiplier = 1.2f;
        public static float hunterAssistanceMult = 1.2f;
        public static IntRange shrineDistance = new IntRange(300, 500);
        // todo make configurable
        public static float maxShrineCombatPoints = 10000f;

        // bounty
        public static float envoyDurationTimeForBountyRelationHagglingInDays = 1f;
        public static float itemRestockDurationInDays = 1f;
        public static float alliedAssistanceDurationInDays = 1f;
        public static float veteranResetTimeInDays = 3f;
        public static bool allowBountyFromBuildingInstigators = true;
        public static bool allowBuyingBountyWithSilver = true;
        public static float bountyValueMult = 0.25f;

        // general
        public static bool autoRemoveAbandondSettlementRuins = true;
        public static bool buffSettlementFoodAndSilverAvailability;
        public static bool buffShrineRewards;

        // categories enabled
        public static bool caravanCampEnabled = true;
        public static bool caravanFormingFilterSelectionEnabled = true;
        public static bool caravanTravelCompanionsEnabled = true;
        public static bool bountyEnabled = true;
        public static bool caravanIncidentsEnabled = true;

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
            Scribe_Values.Look(ref generateStorageForAllInventory, "generateStorageForAllInventory", true);
            Scribe_Values.Look(ref letAnimalsRunFree, "letAnimalsRunFree", false);
            Scribe_Values.Look(ref hasProductionTent, "hasProductionTent", true);
            Scribe_Values.Look(ref hasStorageTent, "hasStorageTent", true);
            Scribe_Values.Look(ref hasMedicalTent, "hasMedicalTent", true);
            Scribe_Values.Look(ref hasAnimalArea, "hasAnimalArea", true);
            Scribe_Values.Look(ref hasPrisonTent, "hasPrisonTent", true);
            Scribe_Values.Look(ref hasPlantTent, "hasPlantTent", true);
            Scribe_Values.Look(ref hasSupplyCostsDisabled, "hasSupplyCostsDisabled", false);
            Scribe_Values.Look(ref autoApplyCampGearRecipes, "autoApplyCampGearRecipes", true);
            Scribe_Values.Look(ref autoApplyCampClearSnowArea, "autoApplyCampClearSnowArea", false);
            Scribe_Values.Look(ref useCustomMapSize, "useCustomMapSize", false);
            Scribe_Values.Look(ref campMapSize, "campMapSize", new IntVec3(250, 1, 250));
            Scribe_Values.Look(ref maxCampSupplyCost, "maxCampSupplyCost", 75);
            Scribe_Values.Look(ref fuelStartingFillPercentage, "fuelStartingFillPercentage", 50);
            Scribe_Values.Look(ref showSupplyCostsInGizmo, "showSupplyCostsInGizmo", true);
            Scribe_Values.Look(ref decorativeFencePosts, "decorativeFencePosts", true);
            Scribe_Values.Look(ref useAnimalOnlyFoodForAnimalArea, "useAnimalOnlyFoodForAnimalArea", false);

            // abilities
            // - ancient gift
            Scribe_Values.Look(ref ancientGiftPassivePsyfocusGainPerSec, "ancientGiftPassivePsyfocusGainPerSec", 0.00035f);
            // - thunderbolt
            Scribe_Values.Look(ref mechanoidDissmemberChance, "mechanoidDissmemberChance", 0.6f);
            Scribe_Values.Look(ref humanDissmemberChance, "humanDissmemberChance", 0.3f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMin, "additionalBuildingAreaDamageMin", 0.5f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMax, "additionalBuildingAreaDamageMax", 0.75f);
            // - meditation
            Scribe_Values.Look(ref psyfocusToRestore, "psyfocusToRestore", 0.15f);
            Scribe_Values.Look(ref plantScoreMultiplier, "plantScoreMultiplier", 2.25f);
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
            Scribe_Values.Look(ref apocalypseEnabled, "apocalypseEnabled", true);
            Scribe_Values.Look(ref apocalypseTemperatureChangePerDay, "apocalypseTemperatureChangePerDay", -0.084f);
            Scribe_Values.Look(ref issueFurtherShrineLocationsAfterStoryEnd, "issueFurtherShrineLocationsAfterStoryEnd", true);
            Scribe_Values.Look(ref sacHuntersHostileTowardsEmpire, "sacHuntersHostileTowardsEmpire", false);
            Scribe_Values.Look(ref allowApocToAlterTileTemp, "allowApocToAlterTileTemp", false);

            // shrines
            Scribe_Values.Look(ref shrineMechDifficultyMultiplier, "shrineMechDifficultyMultiplier", 1.2f);
            Scribe_Values.Look(ref hunterAssistanceMult, "hunterAssistanceMult", 1.2f);
            Scribe_Values.Look(ref shrineDistance, "shrineDistance", new IntRange(300, 500));
            //Scribe_Values.Look(ref maxShrineCombatPoints, "maxShrineCombatPoints", 10000f);

            // bounty
            Scribe_Values.Look(ref envoyDurationTimeForBountyRelationHagglingInDays, "envoyDurationTimeForBountyRelationHagglingInDays", 1f);
            Scribe_Values.Look(ref itemRestockDurationInDays, "itemRestockDurationInDays", 1f);
            Scribe_Values.Look(ref alliedAssistanceDurationInDays, "alliedAssistanceDurationInDays", 1f);
            Scribe_Values.Look(ref veteranResetTimeInDays, "veteranResetTimeInDays", 3f);
            Scribe_Values.Look(ref allowBountyFromBuildingInstigators, "allowBountyFromBuildingInstigators", true);
            Scribe_Values.Look(ref allowBuyingBountyWithSilver, "allowBuyingBountyWithSilver", true);
            Scribe_Values.Look(ref bountyValueMult, "bountyValueMult", 0.25f);

            //general 
            Scribe_Values.Look(ref autoRemoveAbandondSettlementRuins, "autoRemoveAbandondSettlementRuins", true);
            Scribe_Values.Look(ref buffSettlementFoodAndSilverAvailability, "buffSettlementFoodAndSilverAvailability", true);
            Scribe_Values.Look(ref buffShrineRewards, "buffShrineRewards", true);

            // categories enabled
            Scribe_Values.Look(ref caravanCampEnabled, "caravanCampEnabled", true);
            Scribe_Values.Look(ref caravanFormingFilterSelectionEnabled, "caravanFormingFilterSelectionEnabled", true);
            Scribe_Values.Look(ref caravanTravelCompanionsEnabled, "caravanTravelCompanionsEnabled", true);
            Scribe_Values.Look(ref bountyEnabled, "bountyEnabled", true);
            Scribe_Values.Look(ref caravanIncidentsEnabled, "caravanIncidentsEnabled", true);
        }

        public static Rect BRect(Rect rect)
        {
            lastRect = rect;
            return rect;
        }
        public static Rect BRect(float height)
        {
            lastRect = new Rect(lastRect.x, lastRect.y + height, lastRect.width, lastRect.height);
            return lastRect;
        }
        public static Rect BRect(Rect rect, float height)
        {
            lastRect = new Rect(rect.x, rect.y + height, rect.width, rect.height);
            return lastRect;
        }
        public static Rect BRect(float x, float y, float width, float height)
        {
            lastRect = new Rect(x, y, width, height);
            return lastRect;
        }

        private bool showRestartReminder = false;

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);
            //GUI.BeginGroup(wrect);
            //Widgets.BeginScrollView(wrect, ref this.scrollPos, new Rect(0f, 0f, wrect.width, 700f));

            var viewRect = new Rect(0f, 0f, wrect.width, 760);
            options.BeginScrollView(wrect, ref this.scrollPos, ref viewRect);

            var debugRect = BRect(options.GetRect(Text.LineHeight));
            //options.CheckboxLabeled("Debug mode", ref debug);
            //options.CheckboxLabeled("Debug messages", ref debugMessages);

            //if (options.ButtonText("Reset final shrine flags")) StoryUtility.ResetLastShrineFlags();
            //if (options.ButtonText("Print world pawns")) Helper.PrintWorldPawns();
            //if (options.ButtonText("Reset full story")) StoryUtility.RestartStory();

            Widgets.CheckboxLabeled(BRect(0, lastRect.y, options.ColumnWidth / 5 - 20, lastRect.height), "Debug mode", ref debug);
            Widgets.CheckboxLabeled(BRect(options.ColumnWidth / 5 * 1, lastRect.y, options.ColumnWidth / 5 - 20, lastRect.height), "Debug messages", ref debugMessages);
            if (debug)
            {
                if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 2, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Reset final shrine flags")) StoryUtility.ResetLastShrineFlags();
                if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 3, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Print world pawns")) Helper.PrintWorldPawns();
                if (Widgets.ButtonText(BRect(options.ColumnWidth / 5 * 4, lastRect.y, options.ColumnWidth / 5 - 10, lastRect.height), "Reset full story")) StoryUtility.RestartStory();
            }

            options.GapLine();
            var checkRestartRequiringValueChanged = false;

            Text.Font = GameFont.Medium;
            var cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Caravan camping & Caravan improvements".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref CheckForRestart(ref caravanCampEnabled, ref checkRestartRequiringValueChanged));
            if (caravanCampEnabled != checkRestartRequiringValueChanged) showRestartReminder = true;
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open", true, true, true)) Find.WindowStack.Add(new SettingsCamp());
            options.Gap(10);
            options.Label($"Enables settling with a camp, settling with items still packed and the ability to drop them later on and nighttravel.\nAdjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Caravan forming / trading presets & Filter settings".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref CheckForRestart(ref caravanFormingFilterSelectionEnabled, ref checkRestartRequiringValueChanged));
            if (caravanFormingFilterSelectionEnabled != checkRestartRequiringValueChanged) showRestartReminder = true;
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsFilters());
            options.Gap(10);
            options.Label($"Adds new filter presets to the caravan forming and trade dialogs the player can use to quickly select all items he wants to use. Also allows to disable auto supply selection. Fully customizable presets in options.");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Mechanoid Bounty".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref CheckForRestart(ref bountyEnabled, ref checkRestartRequiringValueChanged));
            if (bountyEnabled != checkRestartRequiringValueChanged) showRestartReminder = true;
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsBounty());
            options.Gap(10);
            options.Label($"Adjust settings related to the bounty hunting efforts against mechanoids. Settings for the bounty component of the story, which is used as a standalone, so a vanilla faction, the player can choose, will call out the bounty if royalty isn't isntalled.");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Caravan immersion - Travel companions".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref caravanTravelCompanionsEnabled);
            options.Gap(10);
            options.Label($"Colonists will start out disliking new arrivals to your group, but grow to accept, welcome and maybe embrace them as true friends over time.");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Caravan Incidents - (testing)".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref CheckForRestart(ref caravanIncidentsEnabled, ref checkRestartRequiringValueChanged));
            if (caravanIncidentsEnabled != checkRestartRequiringValueChanged) showRestartReminder = true;
            options.Gap(10);
            options.Label($"Additional Caravan incidents - (only 1 in testing currently, more coming with the next expansion)");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Story settings".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(options.ColumnWidth - 400, lastRect.y + 10, 110, Text.LineHeight), "Enabled: ", ref CheckForRestart(ref storyEnabled, ref checkRestartRequiringValueChanged));

            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsStory());
            options.Gap(10);
            options.Label($"Story settings. Requires Royalty.");
            options.GapLine();
            if (!ModsConfig.RoyaltyActive && storyEnabled) Find.WindowStack.Add(new Dialog_MessageBox(
                new TaggedString($"\nSorry, but the story relies upon and requires Royalty and will be disabled. However all other mod categories are still available.\n\nOnce you have Royalty enabled, you can enable the story here.\nThanks for understanding."),
                "Gotcha",
                () => { if (showRestartReminder) showRestartReminder = false; },
                null,
                null,
                "Missing DLC notification!"));
            else if (checkRestartRequiringValueChanged != storyEnabled) showRestartReminder = true;
            storyEnabled = ModsConfig.RoyaltyActive && storyEnabled;

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"Ancient abilitiy settings".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsAbilities());
            options.Gap(10);
            options.Label($"Adjust settings related to the automatic camp generation. Depending if the player has enough camp supplies, the player's pawns will either build a high quality camp, or a wanting camp construted with makeshift materials.");
            options.GapLine();

            Text.Font = GameFont.Medium;
            cRect = BRect(options.GetRect(Text.LineHeight));
            Widgets.Label(cRect, $"General settings".HtmlFormatting("00ff00", false, 18));
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(options.ColumnWidth - 225, lastRect.y + 6, 150, Text.LineHeight + 10), "Open")) Find.WindowStack.Add(new SettingsOther());
            options.Gap(10);
            options.Label($"A collection of more general, small impovements that help support a life on the road.");
            options.GapLine();

            if (showRestartReminder)
            {
                showRestartReminder = false;
                Find.WindowStack.Add(new Dialog_MessageBox(
                new TaggedString($"\nToggling a mod category requires a game restart to take effect.\n\nSome sub-category settings may also require a restart, but do not get a specific notification."),
                "Gotcha",
                () => { },
                null,
                null,
                "Restart reminder"));
            }

            options.EndScrollView(ref viewRect);
            //Widgets.EndScrollView();
            //GUI.EndGroup();
            options.End();
            this.Write();
        }

        private ref bool CheckForRestart(ref bool setting, ref bool checkRestart)
        {
            checkRestart = setting;
            return ref setting;
        }

    }
}
