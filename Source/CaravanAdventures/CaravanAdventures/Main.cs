﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using CaravanAdventures.CaravanItemSelection;
using CaravanAdventures.Patches;

namespace CaravanAdventures
{
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings>();
            MainPatcher.Patch();
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
