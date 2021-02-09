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

            //var viewRect = new Rect(0f, 0f, windowRect.width - 150, 1200f);
            //options.BeginScrollView(wrect, ref scrollPos, ref viewRect);

            Text.Font = GameFont.Medium;
            options.Label("Camp settings:".Colorize(Color.green), 40f);
            Text.Font = GameFont.Small;

            //options.CheckboxLabeled("Select the tent types you want ur pawns to build by themselfs before you get to ")

            var rect = options.GetRect(Text.LineHeight);

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
