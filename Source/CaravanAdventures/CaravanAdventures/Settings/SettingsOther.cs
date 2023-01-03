using CaravanAdventures.CaravanAbilities;
using CaravanAdventures.Patches;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using System;
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

            options.CheckboxLabeled("Auto-cleanup for abandoned settlements by the player", ref ModSettings.autoRemoveAbandondSettlementRuins, "Doesn't necessarily influence camp behaviour, camps won't leave ruins when using the center spots 'leave immediately' option");
            options.CheckboxLabeled("Increase npc settlement's available silver and food by 2-3 and 1.5-2, respectively", ref ModSettings.buffSettlementFoodAndSilverAvailability);
            options.CheckboxLabeled("Increased ancient shrines and reward chance (hover for more info)", ref ModSettings.buffShrineRewards, "Adds the possibility for an additional ancient danger to appear and adds a chance for a second valuable item when raiding ancient shrines (heavily nerfed for story master shrines)");
            options.CheckboxLabeled("Increase skill point decay level from 10 to 15", ref ModSettings.spDecayLevelIncrease, "Increases the level to 15, when the skill decay starts to set in, so your travelers have it easier being on the road for longer durations");
            options.CheckboxLabeled("Show additional Psycaster or Tough trait info on Caravan dialog", ref ModSettings.showAdditionalPawnStatsInCaravanFormDialog);
            options.Gap();
            options.Label($"Helping settings that have nothing to do with traveling but improve general gameplay:");
            options.CheckboxLabeled("Show letter removal icon", ref ModSettings.showLetterRemoval);
            options.CheckboxLabeled($"Increase firefoam popper detection range from 3 to 5 cells", ref ModSettings.increaseFireFoamPopperDetectionRange);

            options.CheckboxLabeled($"{(ModSettings.keepApparelOnHostileMaps && !Patches.KeepApparelOnHostileMapsPatch.PatchApplied ? "(Requires restart) " : "")}" +
                $"Stop colonists from dropping worn out apparel on non player maps", ref ModSettings.keepApparelOnHostileMaps, 
                $"Disregards the hit-point filtersetting to drop damaged apparel while pawns are on hostile maps so the player can undraft pawns to have them " +
                $"eat and tend to themselves without undressing damaged gear. Only helpful when trying to avoid tattered apparel via filter settings.");

            options.CheckboxLabeled($"Enable pawn title sorter in caravan forming menu.", ref ModSettings.enableSortingByPawnTitle, "Enable an additional sorter for the caravan forming dialog allowing to sort pawns by their title.");
            
            options.Gap();
            options.CheckboxLabeled("Enable button for automatic melee engagements", ref ModSettings.enableEngageMeleeFeature, "Enable button allowing drafted melee pawns to walk up to and attack hostiles themselves");
            options.Gap(6f);
            if (ModSettings.enableEngageMeleeFeature)
            {
                if (!InitPatches.meleeAttackPatched) options.Label($"Please restart the game for auto melee engagements to become available.");
                else
                {
                    var rect = options.GetRect(Text.LineHeight);
                    Widgets.Label(rect, $"Auto engage range in cells: {Math.Round(ModSettings.engageMeleeRange, 0)}");
                    rect.x += (options.ColumnWidth / 3) * 2;
                    rect.width = options.ColumnWidth / 3;
                    Widgets.HorizontalSlider(rect, ref ModSettings.engageMeleeRange, new FloatRange(1f, 50f), Math.Round(ModSettings.engageMeleeRange, 0).ToString(), 0);
                    ModSettings.engageMeleeRange = (float)Math.Round(ModSettings.engageMeleeRange, 0);
                    options.Gap(4f);

                    var ddrect = options.GetRect(Text.LineHeight);
                    Widgets.Label(ddrect, "Default melee engagement behaviour:");

                    ddrect.x += (options.ColumnWidth / 3) * 2;
                    ddrect.width = options.ColumnWidth / 3;

                    Widgets.Dropdown(
                        ddrect,
                        Enum.GetNames(ModSettings.engageMeleeBehaviour.GetType()).Where(x => x != "None"),
                        behaviours => behaviours.FirstOrDefault(),
                        behaviours => behaviours.Select(behaviour => new Widgets.DropdownMenuElement<string> { option = new FloatMenuOption(behaviour, () => ModSettings.engageMeleeBehaviour = (EngageMeleeBehaviour)Enum.Parse(ModSettings.engageMeleeBehaviour.GetType(), behaviour)), payload = behaviour }),
                        new TaggedString($"{Enum.GetName(ModSettings.engageMeleeBehaviour.GetType(), ModSettings.engageMeleeBehaviour)}"));
                }
            }

            options.End();
        }

    }
}
