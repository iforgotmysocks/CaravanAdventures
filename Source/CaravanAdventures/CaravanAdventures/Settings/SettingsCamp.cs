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
    class SettingsCamp : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsCamp()
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


            if (CompCache.BountyWC == null || Find.FactionManager?.AllFactionsListForReading?.FirstOrDefault() == null)
            {
                Log.Warning($"todo label game not loaded");
                options.End();
                return;
            }

            var viewRect = new Rect(0f, 0f, windowRect.width - 150, 1200f);
            options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

            Text.Font = GameFont.Medium;
            options.Label("Camp settings:".HtmlFormatting("ff7777"), 40f);
            var rect = options.GetRect(Text.LineHeight);

            // todo figure out and finish
            Widgets.Dropdown<List<Faction>, Faction>(rect, Find.FactionManager.AllFactions.ToList(), (List<Faction> factions) => { ModSettings.selectedBountyFaction = factions.FirstOrDefault(); return factions.FirstOrDefault(); }, GenerateDropDownElements);

            Text.Font = GameFont.Small;

            options.EndScrollView(ref viewRect);
            options.End();

        }

        private IEnumerable<Widgets.DropdownMenuElement<Faction>> GenerateDropDownElements(List<Faction> factions)
        {
            foreach (var faction in factions)
                yield return new Widgets.DropdownMenuElement<Faction>() { option = new FloatMenuOption(faction.Name, () => Log.Message($"why")), payload = faction };
        }
    }
}
