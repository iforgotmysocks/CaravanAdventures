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
    class SettingsTravelCompanions : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsTravelCompanions()
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
            Text.Font = GameFont.Medium;
            options.Label("Travel companion options:".Colorize(Color.green), 40f);
            Text.Font = GameFont.Small;
            options.CheckboxLabeled("Exclude slaves from travel companion relation functionality", ref ModSettings.excludeSlavesFromTravelCompanions);
            options.End();
        }

    }
}
