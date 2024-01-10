using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CaravanAdventures.CaravanStory.Dialogs
{
    public class NewStorySettings : Page
    {
        private float width;
        private Vector2 scrollPos;
        private float height;

        public NewStorySettings()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = Vector2.zero;

            width = 600f;
            height = 555f;
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - height) / 2f, width, height);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            Text.Font = GameFont.Medium;
            options.Label("Caravan Adventures Setup".Colorize(Color.green), 40f);

            Text.Font = GameFont.Small;
            options.Gap(24);

            options.Label($"Welcome to Caravan Adventures! Please select your preferred storymode: ");
            options.Gap(24);
            var widthSize = options.ColumnWidth * 0.2f;
            Text.Font = GameFont.Medium;
            GUI.color = Color.green;
            var rect = options.GetRect(Text.LineHeight + 6);
            rect.width = widthSize;
            rect.x = widthSize;

            if (ModSettings.storyMode == StoryMode.Normal) GUI.color = Color.white;
            else GUI.color = Color.gray;

            if (Widgets.ButtonText(rect, "Normal", drawBackground: true, doMouseoverSound: true, active: ModSettings.storyMode != StoryMode.Normal, textColor: ModSettings.storyMode == StoryMode.Normal ? Color.green : Color.gray)) ModSettings.storyMode = StoryMode.Normal;

            rect.x += widthSize * 2 - 12;
            rect.width += 35;

            if (ModSettings.storyMode == StoryMode.Performance) GUI.color = Color.white;
            else GUI.color = Color.gray;
            if (Widgets.ButtonText(rect, "Performance", drawBackground: true, doMouseoverSound: true, active: ModSettings.storyMode != StoryMode.Performance, textColor: ModSettings.storyMode == StoryMode.Performance ? Color.green : Color.grey)) ModSettings.storyMode = StoryMode.Performance;

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            options.Gap(24);
            options.Label("Normal: Provides the default story experience, hard but fun.");
            options.Gap();
            options.Label("Performance: Reduces the amount of pawns and enemies spawned by a siginificant amount, and may be preferable when utilizing aging hardware or huge mod lists.");
            options.Gap(24);
            options.Label($"Check CA's mod settings for many more options or to enable/disable features to fit your playthrough. Have fun on your adventure!");
            options.Gap(24);

            var buttonRect = options.GetRect(Text.LineHeight + 6);
            buttonRect.width = 150;
            if (Widgets.ButtonText(buttonRect, "Wiki on Github")) Application.OpenURL("https://github.com/iforgotmysocks/CaravanAdventures/wiki");

            options.Gap();
            buttonRect = options.GetRect(Text.LineHeight + 6);
            buttonRect.width = 170;

            var version = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd(new[] { '.', '0' })}";

            // todo adjust to new version
            if (Widgets.ButtonText(buttonRect, $"Patchnotes on Github")) Application.OpenURL("https://github.com/iforgotmysocks/CaravanAdventures/releases");

            options.Gap();
            var checkboxRect = options.GetRect(Text.LineHeight + 6);
            checkboxRect.width = 300;
            Widgets.CheckboxLabeled(checkboxRect, "Disable this window for further story starts", ref ModSettings.disableSetupWindow);

            options.Gap();
            GUI.color = Color.gray;
            options.Label(version);
            GUI.color = Color.white;
            options.End();
            //Widgets.EndScrollView();
        }

        private IEnumerable<Widgets.DropdownMenuElement<StoryMode>> GenerateStoryModeDropDownContent(StoryMode target)
        {
            foreach (var difficulty in Enum.GetValues(typeof(StoryMode)).Cast<StoryMode>())
                yield return new Widgets.DropdownMenuElement<StoryMode>() { option = new FloatMenuOption(difficulty.ToString(), () => ModSettings.storyMode = difficulty), payload = difficulty };
        }
    }

}
