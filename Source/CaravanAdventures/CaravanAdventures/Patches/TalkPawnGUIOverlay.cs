using CaravanAdventures.CaravanStory;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CaravanAdventures.Patches
{
    class TalkPawnGUIOverlay
    {
        public static void ApplyPatches()
        {
            var drawPawnOrg = AccessTools.Method(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay));
            var drawPawnPost = new HarmonyMethod(typeof(TalkPawnGUIOverlay).GetMethod(nameof(DrawPawnGUIOverlayPostfix)));
            HarmonyPatcher.harmony.Patch(drawPawnOrg, null, drawPawnPost);
        }

        public static void DrawPawnGUIOverlayPostfix(Pawn ___pawn)
        {
            var compTalk = ___pawn.TryGetComp<CompTalk>();
            if (compTalk != null && compTalk.Enabled && compTalk.ShowQuestionMark) ___pawn.Map.overlayDrawer.DrawOverlay(___pawn, OverlayTypes.QuestionMark);
        }

    }
}
