using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections;

namespace CaravanAdventures.Patches
{
    class AutomaticItemSelection
    {
        public static void ApplyPatches(Harmony harmony)
        {   
            var orgOnGUI = AccessTools.Method(typeof(TransferableOneWayWidget), "OnGUI", new Type[]
            {
                typeof(Rect),
                typeof(bool).MakeByRefType()
            });
            var postOnGUI = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(OnGUI_Postfix)));
            harmony.Patch(orgOnGUI, null, postOnGUI);
        }

        public struct Section
        {
            public string title;
            public IEnumerable<TransferableOneWay> transferables;
            public List<TransferableOneWay> cachedTransferables;
        }

        public static void OnGUI_Postfix(TransferableOneWayWidget __instance, List<Section> ___sections, Rect inRect, out bool anythingChanged)
        {
            anythingChanged = false;
            DoOwnButtons(___sections?.FirstOrDefault().cachedTransferables ?? null);
        }

        private static void DoOwnButtons(List<TransferableOneWay> transferables)
        {
            GUI.BeginGroup(new Rect(350f, 0f, 350f, 27f));
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(0f, 0f, 60f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "Select");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
            if (Widgets.ButtonText(rect2, "Pack up", true, true, true))
            {
                // todo create filter for things that make sense - or rather which don't
                foreach (var trans in transferables)
                {
                    trans.AdjustTo(trans.GetMaximumToTransfer());
                }
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, 0f, 130f, 27f), "Short journey", true, true, true))
            {
            }
            GUI.EndGroup();
        }

        
    }

}
