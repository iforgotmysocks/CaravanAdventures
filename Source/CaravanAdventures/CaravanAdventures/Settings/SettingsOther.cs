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
    class SettingsOther : Page
    {
        private float width;

        public SettingsOther()
        {
            doCloseButton = true;
            closeOnCancel = true;

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
            options.Label("General Settings:".Colorize(Color.green), 40f);
            Text.Font = GameFont.Small;

            options.CheckboxLabeled("Auto-cleanup for abandoned settlements by the player", ref ModSettings.autoRemoveAbandondSettlementRuins);
            options.CheckboxLabeled("Increase npc settlement's available silver and food by 2-3 and 1.5-2, respectively", ref ModSettings.buffSettlementFoodAndSilverAvailability);
            options.CheckboxLabeled("Adds the chance for a second valuable item when raiding ancient shrines (heavily nerfed for story master shrines)", ref ModSettings.buffShrineRewards);

            options.End();

        }

    }
}
