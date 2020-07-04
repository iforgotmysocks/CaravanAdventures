using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using CaravanAdventures.CaravanItemSelection;

namespace CaravanAdventures.Patches
{
    public struct Section
    {
        public string title;
        public IEnumerable<TransferableOneWay> transferables;
        public List<TransferableOneWay> cachedTransferables;
    }

    class AutomaticItemSelection
    {
        private static List<Pawn> caravanMembers;
        public static void ApplyPatches(Harmony harmony)
        {
            var orgOnGUI = AccessTools.Method(typeof(TransferableOneWayWidget), "OnGUI", new Type[]
            {
                typeof(Rect),
                typeof(bool).MakeByRefType()
            });
            var preOnGUI = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(OnGUI_Prefix)));
            var postOnGUI = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(OnGUI_Postfix)));
            harmony.Patch(orgOnGUI, null, postOnGUI);
        }

        public static bool OnGUI_Prefix(TransferableOneWayWidget __instance, List<Section> ___sections, out List<Section> __state, Rect inRect, out bool anythingChanged)
        {
            __state = new List<Section>(___sections);
            anythingChanged = false;
            return true;
        }

        public static void OnGUI_Postfix(TransferableOneWayWidget __instance, List<Section> ___sections, List<Section> __state, Rect inRect, ref bool anythingChanged)
        {
            UpdatePeopleSection(___sections);
            DoOwnButtons(___sections, ref anythingChanged);
        }

        private static void UpdatePeopleSection(List<Section> ___sections)
        {
            var detectedPeople = ___sections.SelectMany(section => section.cachedTransferables.Where(trans =>
            {
                var pawn = trans.AnyThing as Pawn;
                if (pawn == null) return false;
                if (pawn.RaceProps.Humanlike && trans.CountToTransfer > 0) return true;
                return false;
            }).Select(trans => (Pawn)trans.AnyThing)
            ).ToList();

            if (detectedPeople.Count > 0) caravanMembers = detectedPeople;
        }

        private static void DoOwnButtons(List<Section> sections, ref bool anythingChanged)
        {
            GUI.BeginGroup(new Rect(350f, 0f, 460f, 27f));
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(0f, 0f, 60f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "Select");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 10f, 0f, 40f, 27f);
            if (Widgets.ButtonText(rect2, "All", true, true, true))
            {
                // todo create filter for things that make sense - or rather which don't
                FilterCombs.ApplyAll(sections);
                anythingChanged = true;
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 5f, 0f, 70f, 27f), "Pack up", true, true, true))
            {

                FilterCombs.ApplyPackUp(sections);
                anythingChanged = true;
            }
           
            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f + 70f, 0f, 70f, 27f), "Goods", true, true, true))
            {
                FilterCombs.ApplyGoods(sections);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 15f + 140f, 0f, 85f, 27f), "Add supplies", true, true, true))
            {
                // todo figure out pplcount?
                FilterCombs.ApplyJourney(sections, caravanMembers);

                anythingChanged = true;
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 20f + 225f, 0f, 50f, 27f), "Clear", true, true, true))
            {
                FilterCombs.ApplyNone(sections);
                anythingChanged = true;
            }
            GUI.EndGroup();
        }

        
    }

}
