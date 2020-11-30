﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Settings
{
    class SettingsAbilities : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsAbilities()
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
            options.Label("Ancient Abilities:".Colorize(Color.red), 40f);

            Text.Font = GameFont.Small;
            options.Label("Ancient thunderbolt:".Colorize(Color.green), 24f);
            options.Label("Mechanoid bodypart dissmember chance: " + Convert.ToInt32(ModSettings.mechanoidDissmemberChance * 100) + "%");
            ModSettings.mechanoidDissmemberChance = options.Slider(ModSettings.mechanoidDissmemberChance, 0f, 1f);
            options.Gap();
            options.Label("Human bodypart dissmember chance: " + Convert.ToInt32(ModSettings.humanDissmemberChance * 100) + "%");
            ModSettings.humanDissmemberChance = options.Slider(ModSettings.humanDissmemberChance, 0f, 1f);
            options.Gap();
            options.Label("Additional minimum damage to buildings: " + Convert.ToInt32(ModSettings.additionalBuildingAreaDamageMin * 100) + "%");
            ModSettings.additionalBuildingAreaDamageMin = options.Slider(ModSettings.additionalBuildingAreaDamageMin, 0f, 1f);
            options.Gap();
            options.Label("Additional maximum damage to buildings: " + Convert.ToInt32(ModSettings.additionalBuildingAreaDamageMax * 100) + "%");
            ModSettings.additionalBuildingAreaDamageMax = options.Slider(ModSettings.additionalBuildingAreaDamageMax, 0f, 1f);

            options.Gap(24f);

            options.Label("Ancient meditation".Colorize(Color.green));
            options.Label($"Psyfocus restored: {Convert.ToInt32(ModSettings.psyfocusToRestore * 100)}% + up to around 25% for drained nature.");
            ModSettings.psyfocusToRestore = options.Slider(ModSettings.psyfocusToRestore, 0f, 1f);

            options.Gap(24f);

            options.Label("Ancient protective aura".Colorize(Color.green));
            options.Label($"Damage healed per second: {Math.Round(ModSettings.healingPerSecond, 2)}");
            ModSettings.healingPerSecond = options.Slider(ModSettings.healingPerSecond, 0f, 1f);
            options.CheckboxLabeled("Can stop mental breaks?", ref ModSettings.stopMentalBreaks);
            options.CheckboxLabeled("Only heal permanent wounds when pawn has ancient gift?", ref ModSettings.onlyHealPermWhenGifted);

            options.Gap(24f);

            options.Label("Mystical light".Colorize(Color.green));
            options.Label($"Duration in seconds: {Math.Round(ModSettings.lightDuration / 60, 0)}");
            ModSettings.lightDuration = options.Slider(ModSettings.lightDuration, 60f, 14400f);


            options.EndScrollView(ref viewRect);
            options.End();

        }

    }
}
