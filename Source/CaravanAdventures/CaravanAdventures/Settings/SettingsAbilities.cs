using RimWorld;
using System;
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
            var viewRect = new Rect(0f, 0f, windowRect.width - 65, 1300);
            var smallerOutRect = new Rect(wrect.x, wrect.y, wrect.width, wrect.height - 50);

            //var viewRect = new Rect(0f, 0f, windowRect.width - 150, 850f);
            //options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

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
                options.Label($"Melee attack speed: {CaravanAbilities.HediffComp_AncientGift.AttackSpeedInBonusPercent}%", -1);
                ModSettings.attackspeedMultiplier = options.Slider(ModSettings.attackspeedMultiplier, 1f, 0.1f);
            }
            options.Gap();

            options.Label("Ancient mech signal:".Colorize(Color.green), 24f);
            options.Label($"Set the min and max amount range of scythers that can appear:");
            options.IntRange(ref ModSettings.scytherRange, 1, 10);
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
            options.Label($"Max allowed linked pawns for the coordinator spell: {ModSettings.maxLinkedAuraPawns}");
            ModSettings.maxLinkedAuraPawns = Convert.ToInt32(options.Slider(ModSettings.maxLinkedAuraPawns, 1f, 10f));
            options.CheckboxLabeled("Can stop mental breaks?", ref ModSettings.stopMentalBreaks);
            options.CheckboxLabeled("Only heal permanent wounds when pawn has ancient gift?", ref ModSettings.onlyHealPermWhenGifted);
            options.CheckboxLabeled("Regulate body temperature to keep pawn save from heat and frost", ref ModSettings.regulateBodyTemperature);
            options.CheckboxLabeled("Show protective aura animation", ref ModSettings.enableAncientAuraAnimation);
            options.Gap(24f);

            options.Label("Mystical guiding light".Colorize(Color.green));
            options.Label($"Duration in days: {Math.Round(ModSettings.lightDuration / 60000, 2)}");
            ModSettings.lightDuration = options.Slider(ModSettings.lightDuration, 60f, 300000);
            // not needed in 1.2
            //options.Label($"Caravan travel speed multiplier: {Math.Round(ModSettings.magicLightCaravanSpeedMult, 1)}");
            //ModSettings.magicLightCaravanSpeedMult = options.Slider(ModSettings.magicLightCaravanSpeedMult, 0.1f, 5f);

            options.Gap();

            Text.Font = GameFont.Medium;
            options.Label("Compatibility Settings:".Colorize(Color.green), 30f);
            Text.Font = GameFont.Small;
            options.CheckboxLabeled("(SoS2) Protective Aura prevents Hypoxia", ref ModSettings.sos2AuraPreventsHypoxia);
            options.Label($"(SoS2) Protective Aura Ship heat absorption percentage: {Math.Round(ModSettings.sos2HeatAbsorptionPercentage, 0)}%", -1, "How much of the incoming heat the aura protected pawn can absorb and how much goes through to the ship's network regardless in percent.");
            ModSettings.sos2HeatAbsorptionPercentage = (float)Math.Round(options.Slider(ModSettings.sos2HeatAbsorptionPercentage, 0f, 100f), 0);
            options.Label($"(SoS2) Protective Aura Ship heat absroption per psychic heat point: {Math.Round(ModSettings.sos2AuraHeatMult, 0)}", -1, "Heat generated e.g. by incoming fire hitting shields will be absorbed by a protective aura protected pawn on the ship's map.");
            ModSettings.sos2AuraHeatMult = (float)Math.Round(options.Slider(ModSettings.sos2AuraHeatMult, 0f, 3000f), 0);

            options.End();
            Widgets.EndScrollView();


        }

    }
}
