using CaravanAdventures.CaravanStory;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.Patches
{
    class TalkPawnGUIOverlay
    {
        public static void ApplyPatches(Harmony harmony)
        {
            var drawPawnOrg = AccessTools.Method(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay));
            var drawPawnPost = new HarmonyMethod(typeof(TalkPawnGUIOverlay).GetMethod(nameof(DrawPawnGUIOverlayPostfix)));
            harmony.Patch(drawPawnOrg, null, drawPawnPost);
        }

        public static void DrawPawnGUIOverlayPostfix(Pawn ___pawn)
        {
            var compTalk = ___pawn.TryGetComp<CompTalk>();
            if (compTalk != null && compTalk.Enabled && compTalk.ShowQuestionMark) ___pawn.Map.overlayDrawer.DrawOverlay(___pawn, OverlayTypes.QuestionMark);
        }

    }
}
