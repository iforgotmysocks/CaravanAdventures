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
            var viewRect = new Rect(0f, 0f, windowRect.width - 65, 980);
            var smallerOutRect = new Rect(wrect.x, wrect.y, wrect.width, wrect.height - 50);

            Widgets.BeginScrollView(smallerOutRect, ref scrollPos, viewRect);
            options.Begin(viewRect);
            Text.Font = GameFont.Medium;
            options.Label("Ancient Abilities:".Colorize(Color.green), 40f);

            Text.Font = GameFont.Small;

            options.Label("Ancient Gift:".Colorize(Color.green), 24f);
            options.Label("Passive psyfocus gain per sec: " + Math.Round(ModSettings.ancientGiftPassivePsyfocusGainPerSec * 100, 2) + "%");
            ModSettings.ancientGiftPassivePsyfocusGainPerSec = options.Slider(ModSettings.ancientGiftPassivePsyfocusGainPerSec, 0.00001f, 0.01f);
            options.CheckboxLabeled("Enable melee attack speed boost", ref ModSettings.attackspeedIncreaseForAncientProtectiveAura);
            if (ModSettings.attackspeedIncreaseForAncientProtectiveAura)
            {
                options.Label($"Attack speed boost: {Math.Round(1000 / (ModSettings.attackspeedMultiplier / 0.1) - 100, 0)}%", -1);
                ModSettings.attackspeedMultiplier = options.Slider(ModSettings.attackspeedMultiplier, 1f, 0.1f);
            }
            options.Gap();

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
            options.Label($"Psyfocus multiplier from draining nature: {Math.Round(ModSettings.plantScoreMultiplier, 2)}");
            ModSettings.plantScoreMultiplier = options.Slider(ModSettings.plantScoreMultiplier, 0f, 5f);
            options.Gap(24f);

            options.Label("Ancient protective aura".Colorize(Color.green));
            options.Label($"Damage healed per second: {Math.Round(ModSettings.healingPerSecond, 2)}");
            ModSettings.healingPerSecond = options.Slider(ModSettings.healingPerSecond, 0f, 1f);
            options.Label($"Max allowed linked pawns: {ModSettings.maxLinkedAuraPawns}");
            ModSettings.maxLinkedAuraPawns = Convert.ToInt32(options.Slider(ModSettings.maxLinkedAuraPawns, 1f, 10f));
            options.CheckboxLabeled("Exclude slaves from the coordinator empowered version of the ancient aura", ref ModSettings.excludeSlavesFromCoordinator);
            options.CheckboxLabeled("Can stop mental breaks?", ref ModSettings.stopMentalBreaks);
            options.CheckboxLabeled("Only heal permanent wounds when pawn has ancient gift?", ref ModSettings.onlyHealPermWhenGifted);
            options.Gap(24f);

            options.Label("Mystical guiding light".Colorize(Color.green));
            options.Label($"Duration in days: {Math.Round(ModSettings.lightDuration / 60000, 2)}");
            ModSettings.lightDuration = options.Slider(ModSettings.lightDuration, 60f, 300000);
            options.Label($"Caravan travel speed multiplier: {Math.Round(ModSettings.magicLightCaravanSpeedMult, 1)}");
            ModSettings.magicLightCaravanSpeedMult = options.Slider(ModSettings.magicLightCaravanSpeedMult, 0.1f, 5f);

            options.End();
            Widgets.EndScrollView();
        }

    }
}
