﻿using RimWorld;
using UnityEngine;
using Verse;
using CaravanAdventures.CaravanItemSelection;
using System.Collections.Generic;

namespace CaravanAdventures.Settings
{
    class SettingsFilters : Page
    {
        private float width;
        //private Vector2 scrollPos, scrollPosPackup, scrollPosGoods, scrollPosJourney;
        private ThingFilterUI.UIState scrollPos, scrollPosPackup, scrollPosGoods, scrollPosJourney;

        public SettingsFilters()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = new ThingFilterUI.UIState();
            scrollPosPackup = new ThingFilterUI.UIState();
            scrollPosGoods = new ThingFilterUI.UIState();
            scrollPosJourney = new ThingFilterUI.UIState();

            scrollPos.scrollPosition = scrollPosPackup.scrollPosition = scrollPosGoods.scrollPosition = scrollPosJourney.scrollPosition = Vector2.zero;

            width = UI.screenWidth < 900 ? UI.screenWidth : 900;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - InitialSize.y) / 2f, width, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            var optionHeading = new Listing_Standard();
            //optionHeading.Begin(new Rect(wrect.x, wrect.y, wrect.width, 200f));
            optionHeading.Begin(wrect);

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
            optionHeading.Gap(28);
            var textLabelRect = ModSettings.BRect(optionHeading.GetRect(Text.LineHeight * 4));
            textLabelRect.width = optionHeading.ColumnWidth * 0.64f;
            Widgets.Label(textLabelRect, "Here you can adjust what items the specific filters should cover, which can simply be applied by clicking the specific button in the caravan forming or trading dialog. Further filter options like quality or tainted are applied aswell. The \"Exclude other items\" option sets whether selecting the preset should reset previously made selections or not.");
            Widgets.CheckboxLabeled(ModSettings.BRect(optionHeading.ColumnWidth / 3 * 2.1f, ModSettings.lastRect.y, optionHeading.ColumnWidth / 3 * 0.8f, Text.LineHeight + 4), "Toggle selected caravan pawns", ref ModSettings.autoSelectPawns);
            Widgets.CheckboxLabeled(ModSettings.BRect(optionHeading.ColumnWidth / 3 * 2.1f, ModSettings.lastRect.y + Text.LineHeight + 4, optionHeading.ColumnWidth / 3 * 0.8f, Text.LineHeight + 4), "Toggle selected caravan items", ref ModSettings.autoSelectItems);
            optionHeading.Gap();
            optionHeading.End();
            
            var options = new Listing_Standard();
            options.ColumnWidth = (windowRect.width / 3) - 30f;
            options.Begin(new Rect(wrect.x, wrect.y + optionHeading.CurHeight, wrect.width, wrect.height));

            options.Label("Pack up:".Colorize(Color.green), 24f);
            var packUpRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(packUpRect, scrollPosPackup, InitGC.packUpFilter, ThingFilter.CreateOnlyEverStorableThingFilter());
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.packUpExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            options.NewColumn();

            options.Label("Goods:".Colorize(Color.green), 24f);
            var goodsRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(goodsRect, scrollPosGoods, InitGC.goodsFilter, ThingFilter.CreateOnlyEverStorableThingFilter());
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.goodsExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            options.NewColumn();

            options.Label("Goods2:".Colorize(Color.green), 24f);
            var journeyRect = options.GetRect(400);
            ThingFilterUI.DoThingFilterConfigWindow(journeyRect, scrollPosJourney, InitGC.journeyFilter, ThingFilter.CreateOnlyEverStorableThingFilter());
            options.Gap();
            options.CheckboxLabeled("Exclude other items", ref InitGC.journeyExclusive, "When active, the button will disable other items instead of just adding the filtered ones");
            options.Gap();

            options.End();
            // todo
            //var footer = new Listing_Standard();
            //footer.Begin(new Rect(wrect.x, wrect.y + options.CurHeight, wrect.width, wrect.height));
            //var footerRect = ModSettings.BRect(footer.GetRect(Text.LineHeight));
            //Widgets.CheckboxLabeled(footerRect, "Auto enable selected map pawns in caravan forming dialogs", ref ModSettings.autoSelectPawns);
            //footer.End();
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
