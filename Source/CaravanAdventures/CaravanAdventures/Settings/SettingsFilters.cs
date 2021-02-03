using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using CaravanAdventures.CaravanItemSelection;

namespace CaravanAdventures.Settings
{
    class SettingsFilters : Page
    {
        private float width;
        private Vector2 scrollPos, scrollPosPackup, scrollPosGoods, scrollPosJourney;

        public SettingsFilters()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = scrollPosPackup = scrollPosGoods = scrollPosJourney = Vector2.zero;

            width = UI.screenWidth < 900 ? UI.screenWidth : 900;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - InitialSize.y) / 2f, width, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            // todo finish
            var optionHeading = new Listing_Standard();
            optionHeading.Begin(new Rect(wrect.x, wrect.y, wrect.width, 180));

            //var viewRect = new Rect(0f, 0f, wrect.width, 1600);
            //options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

            Text.Font = GameFont.Medium;
            optionHeading.Label("Filter settings:".Colorize(Color.green), 40f);
            optionHeading.Gap();

            if (CompCache.InitGC == null)
            {
                optionHeading.Label($"Filter settings are savegame specific, please load up a savegame to adjust the filters. Thanks.");
                optionHeading.End();
                return;
            }

            Text.Font = GameFont.Small;
            var cRect = ModSettings.BRect(optionHeading.GetRect(Text.LineHeight));
            Widgets.Label(cRect, "Filter settings are saved within a savegame, so don't forget to save to keep ur changes");
            if (InitGC.packUpFilter == null || InitGC.goodsFilter == null || Widgets.ButtonText(ModSettings.BRect(optionHeading.ColumnWidth * 0.8f, ModSettings.lastRect.y - 4, optionHeading.ColumnWidth * 0.15f, Text.LineHeight + 10), "Restore defaults")) RestoreFilterDefaults();
            optionHeading.Gap();
            optionHeading.End();
            
            var options = new Listing_Standard();
            options.ColumnWidth = (windowRect.width / 3) - 30f;
            options.Begin(new Rect(wrect.x, wrect.y + optionHeading.CurHeight, wrect.width, wrect.height));

            options.Label("Pack up:".Colorize(Color.green), 24f);
            var packUpRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(packUpRect, ref scrollPosPackup, InitGC.packUpFilter);
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.packUpExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            options.NewColumn();

            options.Label("Goods:".Colorize(Color.green), 24f);
            var goodsRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(goodsRect, ref scrollPosGoods, InitGC.goodsFilter);
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.goodsExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            options.NewColumn();

            options.Label("Goods2:".Colorize(Color.green), 24f);
            var journeyRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(journeyRect, ref scrollPosJourney, InitGC.journeyFilter);
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.journeyExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            //options.EndScrollView(ref viewRect);
            options.End();
        }

        public static void RestoreFilterDefaults()
        {
            InitGC.packUpFilter = new ThingFilter();
            InitGC.goodsFilter = new ThingFilter();
            InitGC.journeyFilter = new ThingFilter();

            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (FilterHelper.DoFiltersApply(FilterCombs.packUp, def)) InitGC.packUpFilter.SetAllow(def, true);
                else InitGC.packUpFilter.SetAllow(def, false);
                InitGC.packUpFilter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
                InitGC.packUpFilter.SetAllow(SpecialThingFilterDef.Named("AllowBiocodedWeapons"), false);

                if (FilterHelper.DoFiltersApply(FilterCombs.goods, def)) InitGC.goodsFilter.SetAllow(def, true);
                else InitGC.goodsFilter.SetAllow(def, false);
                InitGC.goodsFilter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
                InitGC.goodsFilter.SetAllow(SpecialThingFilterDef.Named("AllowBiocodedWeapons"), false);
                InitGC.goodsFilter.AllowedQualityLevels = new QualityRange(QualityCategory.Awful, QualityCategory.Excellent);

                if (FilterHelper.DoFiltersApply(FilterCombs.goods, def)) InitGC.journeyFilter.SetAllow(def, true);
                else InitGC.journeyFilter.SetAllow(def, false);
                InitGC.journeyFilter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
                InitGC.journeyFilter.SetAllow(SpecialThingFilterDef.Named("AllowBiocodedWeapons"), false);
                //InitGC.goodsFilter.AllowedQualityLevels = new QualityRange(QualityCategory.Awful, QualityCategory.Excellent);
            }
        }
    }
}
