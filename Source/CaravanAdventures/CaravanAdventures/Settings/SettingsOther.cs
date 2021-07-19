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

            options.CheckboxLabeled("Auto-cleanup for abandoned settlements by the player", ref ModSettings.autoRemoveAbandondSettlementRuins, "Doesn't influence camp behaviour, camps won't leave ruins either way");
            options.CheckboxLabeled("Increase npc settlement's available silver and food by 2-3 and 1.5-2, respectively", ref ModSettings.buffSettlementFoodAndSilverAvailability);
            options.CheckboxLabeled("Increased ancient shrines and reward chance (hover for more info)", ref ModSettings.buffShrineRewards, "Adds the possibility for an additional ancient danger to appear and adds a chance for a second valuable item when raiding ancient shrines (heavily nerfed for story master shrines)");
            options.CheckboxLabeled("Increase skill point decay level from 10 to 15", ref ModSettings.spDecayLevelIncrease, "Increases the level to 15, when the skill decay starts to set in, so your travelers have it easier being on the road for longer durations");
            options.CheckboxLabeled("Show letter removal icon", ref ModSettings.showLetterRemoval);
            options.End();
        }

    }
}
