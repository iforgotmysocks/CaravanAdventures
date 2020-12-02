﻿using CaravanAdventures.CaravanStory;
using CaravanAdventures.Settings;
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
        private Rect lastRect = default;

        // general
        public static bool debug = false;

        // camp
        public static bool generateStorageForAllInventory = true;

        // abilities
        // - thunderbolt
        public static float mechanoidDissmemberChance = 0.6f;
        public static float humanDissmemberChance = 0.3f;
        public static float additionalBuildingAreaDamageMin = 0.5f;
        public static float additionalBuildingAreaDamageMax = 0.75f;
        // - meditation
        public static float psyfocusToRestore = 0.3f;
        // - protective aura
        public static float healingPerSecond = 0.05f;
        public static bool onlyHealPermWhenGifted = false;
        public static bool stopMentalBreaks = false;
        // - light
        public static float lightDuration = 1200f;

        // story
        // todo remove the ability to disable the apoc? Can always set the increase to 0
        public static bool storyEnabled = true;
        public static bool limitLargeMapSizesToTestedSize = true;
        public static bool apocalypseEnabled = true;
        public static float apocalypseTemperatureChangePerDay = -0.084f;

        // shrines

        // bounty
        public static float envoyDurationTimeForBountyRelationHagglingInDays = 1f;
        public static float itemRestockDurationInDays = 1f;
        public static float alliedAssistanceDurationInDays = 1f;

        //public static ModSettings Get() => LoadedModManager.GetMod<CaravanAdventures.Main>().GetSettings<ModSettings>();
        private Vector2 scrollPos = Vector2.zero;

        public override void ExposeData()
        {
            base.ExposeData();
            // debug
            Scribe_Values.Look(ref debug, "debug");

            // camp
            Scribe_Values.Look(ref generateStorageForAllInventory, "generateStorageForAllInventory", false);

            // abilities
            // - thunderbolt
            Scribe_Values.Look(ref mechanoidDissmemberChance, "mechanoidDissmemberChance", 0.6f);
            Scribe_Values.Look(ref humanDissmemberChance, "humanDissmemberChance", 0.3f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMin, "additionalBuildingAreaDamageMin", 0.5f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMax, "additionalBuildingAreaDamageMax", 0.75f);
            // - meditation
            Scribe_Values.Look(ref psyfocusToRestore, "psyfocusToRestore", 0.3f);
            // - protective aura
            Scribe_Values.Look(ref healingPerSecond, "healingPerSecond", 0.05f);
            Scribe_Values.Look(ref stopMentalBreaks, "stopMentalBreaks", false);
            Scribe_Values.Look(ref onlyHealPermWhenGifted, "onlyHealPermWhenGifted", false);
            // - light
            Scribe_Values.Look(ref lightDuration, "lightDuration", 1200f);

            // story
            Scribe_Values.Look(ref storyEnabled, "storyEnabled", true);
            Scribe_Values.Look(ref limitLargeMapSizesToTestedSize, "limitLargeMapSizesToTestedSize", true);
            Scribe_Values.Look(ref apocalypseEnabled, "apocalypseEndabled");
            Scribe_Values.Look(ref apocalypseTemperatureChangePerDay, "apocalypseTemperatureChangePerDay", -0.084f);

            // shrines

            // bounty
            Scribe_Values.Look(ref envoyDurationTimeForBountyRelationHagglingInDays, "envoyDurationTimeForBountyRelationHagglingInDays", 1f);
            Scribe_Values.Look(ref itemRestockDurationInDays, "itemRestockDurationInDays", 1f);
            Scribe_Values.Look(ref alliedAssistanceDurationInDays, "alliedAssistanceDurationInDays", 1f);
        }

        public bool toggleTest = false;


        public void DoWindowContentsNew(Rect wrect)
        {
            var options = new Listing_Standard();

            options.Begin(wrect);
            var viewRect = new Rect(0f, 0f, wrect.width, 1200);
            options.BeginScrollView(wrect, ref this.scrollPos, ref viewRect);

            options.Label("Ancient Abilities:".Colorize(Color.red), 40f);
            if (options.ButtonTextLabeled("Ancient ability settings", "Open Ability")) Find.WindowStack.Add(new SettingsAbilities());


            Widgets.CheckboxLabeled(BRect(options.ColumnWidth - 400, options.GetRect(Text.LineHeight).y, 200, Text.LineHeight), "Enabled: ", ref toggleTest);
            Widgets.ButtonText(BRect(options.ColumnWidth - 200, lastRect.y, 200, Text.LineHeight), "button");


            options.Gap();

            options.EndScrollView(ref viewRect);
            options.End();
            this.Write();
        }



        private Rect BRect(float x, float y, float width, float height)
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

            var viewRect = new Rect(0f, 0f, wrect.width, 1200);
            options.BeginScrollView(wrect, ref this.scrollPos, ref viewRect);


            options.CheckboxLabeled("Debug mode", ref debug);
            if (options.ButtonText("Reset full story")) StoryUtility.RestartStory();
            options.CheckboxLabeled("Apocalypse enabled", ref apocalypseEnabled);
            options.Label($"Apocalypse temperature change per day: {Math.Round(apocalypseTemperatureChangePerDay, 4)}  (Default: -0.084)");
            apocalypseTemperatureChangePerDay = options.Slider(apocalypseTemperatureChangePerDay, 0f, -0.5f);
            options.Gap();

            // todo figure out scroll views
            //Rect viewRect = new Rect(0, 0, 500, 3000);
            //Vector2 vec = new Vector2(0, 0);
            //options.BeginScrollView(wrect, ref vec, ref viewRect);
            // todo move to seperate window!

            options.Label("Settlement price adjustments");
            options.Label("Multiplies the current money for settlements with less then 3k silver by 3, and those with more by 2");
            if (options.ButtonTextLabeled("Increase Settlement money", "Increase")) Helper.AdjustSettlementPrices();
            // todo add min and max multiplier used in the def patch for settlement money
            options.Gap();


            options.Label("Ancient thunderbolt:".Colorize(Color.red), 40f);
            options.Label("Mechanoid bodypart dissmember chance: " + Convert.ToInt32(mechanoidDissmemberChance * 100) + "%");
            mechanoidDissmemberChance = options.Slider(mechanoidDissmemberChance, 0f, 1f);
            options.Gap();
            options.Label("Human bodypart dissmember chance: " + Convert.ToInt32(humanDissmemberChance * 100) + "%");
            humanDissmemberChance = options.Slider(humanDissmemberChance, 0f, 1f);
            options.Gap();
            options.Label("Additional minimum damage to buildings: " + Convert.ToInt32(additionalBuildingAreaDamageMin * 100) + "%");
            additionalBuildingAreaDamageMin = options.Slider(additionalBuildingAreaDamageMin, 0f, 1f);
            options.Gap();
            options.Label("Additional maximum damage to buildings: " + Convert.ToInt32(additionalBuildingAreaDamageMax * 100) + "%");
            additionalBuildingAreaDamageMax = options.Slider(additionalBuildingAreaDamageMax, 0f, 1f);

            options.Gap(24f);

            options.Label("Ancient meditation".Colorize(Color.red));
            options.Label($"Psyfocus restored: {Convert.ToInt32(psyfocusToRestore * 100)}% + up to around 25% for drained nature.");
            psyfocusToRestore = options.Slider(psyfocusToRestore, 0f, 1f);

            options.Gap(24f);

            options.Label("Ancient protective aura".Colorize(Color.red));
            options.Label($"Damage healed per second: {Math.Round(healingPerSecond, 2)}");
            healingPerSecond = options.Slider(healingPerSecond, 0f, 1f);
            options.CheckboxLabeled("Can stop mental breaks?", ref stopMentalBreaks);
            options.CheckboxLabeled("Only heal permanent wounds when pawn has ancient gift?", ref onlyHealPermWhenGifted);

            options.Gap(24f);

            options.Label("Mystical light".Colorize(Color.red));
            options.Label($"Duration in seconds: {Math.Round(lightDuration / 60, 0)}");
            lightDuration = options.Slider(lightDuration, 60f, 14400f);

            options.Gap();
            options.CheckboxLabeled("Generate storage tents for all items and unload", ref generateStorageForAllInventory);

            options.EndScrollView(ref viewRect);
            //Widgets.EndScrollView();
            //GUI.EndGroup();
            options.End();
            this.Write();
        }




    }
}
