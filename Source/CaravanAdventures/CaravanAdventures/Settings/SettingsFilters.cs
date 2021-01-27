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
    class SettingsFilters : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsFilters()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = Vector2.zero;

            width = 400f;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - InitialSize.y) / 2f, width, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            // todo finish
            var options = new Listing_Standard();
            options.Begin(wrect);

            Text.Font = GameFont.Medium;
            options.Label("Filter settings:".Colorize(Color.green), 40f);
            Text.Font = GameFont.Small;

            if (CompCache.InitGC == null)
            {
                options.Label($"Filter settings are savegame specific, please load up a savegame to adjust the filters. Thanks.");
                options.End();
                return;
            }

            // todo add generation of standart filters -> possible to reuse existing filter structure?
            if (InitGC.packup == null)
            {
                InitGC.packup = new ThingFilter();
                InitGC.packup.SetFromPreset(StorageSettingsPreset.DefaultStockpile);
            }
          
            options.Label("Pack up:".Colorize(Color.green), 24f);
            var rect = options.GetRect(400);
            if (InitGC.packup != null) ThingFilterUI.DoThingFilterConfigWindow(rect, ref scrollPos, InitGC.packup);
            options.Gap();

            options.Label("Goods:".Colorize(Color.green), 24f);
            rect = options.GetRect(400);
            if (InitGC.packup != null) ThingFilterUI.DoThingFilterConfigWindow(rect, ref scrollPos, InitGC.packup);
            options.Gap();


            options.End();
        }

    }
}
