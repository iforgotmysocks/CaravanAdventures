using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    class TexCustom
    {
        public static readonly Texture2D RemoveLetters = ContentFinder<Texture2D>.Get("UI/Buttons/RemoveLetters", true);
        public static readonly Texture2D Drop = ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true);
        public static readonly Texture2D CaravanSettings = ContentFinder<Texture2D>.Get("UI/Buttons/CaravanSettings", true);
    }

    class CustomIconPatches
    {
        public static void ApplyPatches()
        {
            if (!ModSettings.showLetterRemoval) return;
            var letterOrg = AccessTools.Method(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls));
            var letterPost = new HarmonyMethod(typeof(CustomIconPatches).GetMethod(nameof(PlaySettingsDoPlaySettingsGlobalControlsPostfix)));
            HarmonyPatcher.harmony.Patch(letterOrg, null, letterPost);
        }

        public static void PlaySettingsDoPlaySettingsGlobalControlsPostfix(WidgetRow row, bool worldView)
        {
            if (worldView) return;
            if (row.ButtonIcon(TexCustom.RemoveLetters, "CARemoveLettersButton".Translate())) Find.LetterStack.LettersListForReading.ToList().ForEach(letter => Find.LetterStack.RemoveLetter(letter));
        }
    }
}
