﻿using System;
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
        public float mechanoidDissmemberChance = 0.6f;
        public float humanDissmemberChance = 0.3f;
        public float additionalBuildingAreaDamageMin = 0.5f;
        public float additionalBuildingAreaDamageMax = 0.75f;
        public float psyfocusToRestore = 0.3f;
        public float healingPerSecond = 0.05f;
        public bool stopMentalBreaks = false;
        public float lightDuration = 1200f;

        public static ModSettings Get()
        {
            return LoadedModManager.GetMod<CaravanAdventures.Main>().GetSettings<ModSettings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);


            // todo move to seperate window!
            options.Label("Ancient thunderbolt:".Colorize(Color.red), 40f);
            options.Label("Mechanoid bodypart dissmember chance: " + Convert.ToInt32(mechanoidDissmemberChance * 100) + "%");
            mechanoidDissmemberChance = options.Slider(mechanoidDissmemberChance, 0f, 1f);
            options.Gap();
            options.Label("Humand bodypart dissmember chance: " + Convert.ToInt32(humanDissmemberChance * 100) + "%");
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

            options.Gap(24f);

            options.Label("Mystical light".Colorize(Color.red));
            options.Label($"Duration in seconds: {Math.Round(lightDuration / 60, 0)}");
            lightDuration = options.Slider(lightDuration, 60f, 14400f);

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref mechanoidDissmemberChance, "mechanoidDissmemberChance", 0.5f);
            Scribe_Values.Look(ref humanDissmemberChance, "humanDissmemberChance", 0.3f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMin, "additionalBuildingAreaDamageMin", 0.5f);
            Scribe_Values.Look(ref additionalBuildingAreaDamageMax, "additionalBuildingAreaDamageMax", 0.75f);

            Scribe_Values.Look(ref psyfocusToRestore, "psyfocusToRestore", 0.5f);

            Scribe_Values.Look(ref healingPerSecond, "healingPerSecond", 0.05f);
            Scribe_Values.Look(ref stopMentalBreaks, "stopMentalBreaks", false);

            Scribe_Values.Look(ref lightDuration, "lightDuration", 1200f);
        }
    }
}
