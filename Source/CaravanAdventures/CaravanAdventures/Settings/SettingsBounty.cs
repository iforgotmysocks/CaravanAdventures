﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Settings
{
    class SettingsBounty : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsBounty()
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
            options.Label("Camp settings:".Colorize(Color.green), 40f);
            options.Gap();

            if (CompCache.BountyWC == null || Find.FactionManager?.AllFactionsListForReading?.FirstOrDefault() == null)
            {
                options.Label($"Bounty settings are savegame specific, please load up a savegame to adjust the settings. Thanks.");
                options.End();
                return;
            }
            if (CompCache.BountyWC.BountyFaction == null)
            {
                Log.Warning($"bounty faction null which should not happen");
            }

            //var viewRect = new Rect(0f, 0f, windowRect.width - 150, 1200f);
            //options.BeginScrollView(wrect, ref scrollPos, ref viewRect);


            Text.Font = GameFont.Small;
            options.Gap();
            options.Label("Here bounty settings can be configured.");
            options.Gap();


            options.Label("(Matters when royalty, or rather the story isn't enabled and sacrileg hunters don't exist");
            var rect = options.GetRect(Text.LineHeight);
            Widgets.Label(rect, "Select the desired bounty faction:");

            rect.x += options.ColumnWidth / 2;
            rect.width = options.ColumnWidth / 2;

            // todo figure out and finish
            // dafuq does getPayload do
            Widgets.Dropdown<List<Faction>, Faction>(
                rect, 
                Find.FactionManager.AllFactions.ToList(), 
                (List<Faction> factions) => { return factions.FirstOrDefault(); }, 
                GenerateDropDownElements, 
                CompCache.BountyWC.BountyFaction != null ? new TaggedString($"{CompCache.BountyWC.BountyFaction.NameColored} ({CompCache.BountyWC.BountyFaction.GoodwillWith(Faction.OfPlayerSilentFail)})") : new TaggedString("Please select a faction"));

            // only for testing purposes, remove
            var serviceAvailable = CompCache.BountyWC.BountyServiceAvailable;
            options.CheckboxLabeled("bounty services enabled", ref serviceAvailable);
            CompCache.BountyWC.BountyServiceAvailable = serviceAvailable;

            options.Label($"Envoy duration to improve relations with bounties in days: {Math.Round(ModSettings.envoyDurationTimeForBountyRelationHagglingInDays, 1)}");
            ModSettings.envoyDurationTimeForBountyRelationHagglingInDays = options.Slider(ModSettings.envoyDurationTimeForBountyRelationHagglingInDays, 0.1f, 60f);
            options.Label($"Rare item restock duration in days: {Math.Round(ModSettings.itemRestockDurationInDays, 1)}");
            ModSettings.itemRestockDurationInDays = options.Slider(ModSettings.itemRestockDurationInDays, 0.1f, 60f);
            options.Label($"Allied assistance timeout in days: {Math.Round(ModSettings.alliedAssistanceDurationInDays, 1)}");
            ModSettings.alliedAssistanceDurationInDays = options.Slider(ModSettings.alliedAssistanceDurationInDays, 0.1f, 60f);
            options.Label($"Veteran availability in days: {Math.Round(ModSettings.veteranResetTimeInDays, 1)}");
            ModSettings.veteranResetTimeInDays = options.Slider(ModSettings.veteranResetTimeInDays, 0.1f, 60f);
            options.CheckboxLabeled("Allow bounty gained by buildings (turrets etc)", ref ModSettings.allowBountyFromBuildingInstigators);
            options.CheckboxLabeled("Allow exchanging bounty points for silver", ref ModSettings.allowBuyingBountyWithSilver);
            options.Label($"Bounty value multiplier. (Default: 1 silver ~ 0.25 bounty credit): {Math.Round(ModSettings.bountyValueMult, 2)}");
            ModSettings.bountyValueMult = options.Slider(ModSettings.bountyValueMult, 0.1f, 4f);

            //options.EndScrollView(ref viewRect);
            options.End();

        }

        private IEnumerable<Widgets.DropdownMenuElement<Faction>> GenerateDropDownElements(List<Faction> factions)
        {
            foreach (var faction in factions)
            {
                if (faction.def.permanentEnemy || faction == Faction.OfPlayer || faction.leader == null || faction.def.techLevel < TechLevel.Industrial) continue;
                if (ModSettings.storyEnabled && faction != CaravanStory.StoryUtility.FactionOfSacrilegHunters) continue;
                yield return new Widgets.DropdownMenuElement<Faction>() { option = new FloatMenuOption(faction.Name, () => CompCache.BountyWC.BountyFaction = faction), payload = faction };
            }
        }
    }
}
