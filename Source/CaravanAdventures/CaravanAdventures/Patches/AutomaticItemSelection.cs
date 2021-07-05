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
            if (!ModSettings.caravanFormingFilterSelectionEnabled) return;
            var orgOnGUI = AccessTools.Method(typeof(TransferableOneWayWidget), "OnGUI", new Type[]
            {
                typeof(Rect),
                typeof(bool).MakeByRefType()
            });
            var postOnGUI = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(OnGUI_Postfix)));
            harmony.Patch(orgOnGUI, null, postOnGUI);

            var orgDoWindowContents = AccessTools.Method(typeof(Dialog_Trade), "DoWindowContents", new Type[]
            {
                typeof(Rect),
            });
            var postDoWindowContents = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(DoWindowContents_Postfix)));
            harmony.Patch(orgDoWindowContents, null, postDoWindowContents);

            var orgDialog_FormCaravan = AccessTools.DeclaredConstructor(typeof(Dialog_FormCaravan), new[] {
                typeof(Map),
                typeof(bool),
                typeof(Action),
                typeof(bool)
            });
            var postDialog_FormCaravan = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(Dialog_FormCaravan_Postfix)));
            harmony.Patch(orgDialog_FormCaravan, null, postDialog_FormCaravan);
        }

        public static void OnGUI_Postfix(TransferableOneWayWidget __instance, List<Section> ___sections, List<Section> __state, Rect inRect, ref bool anythingChanged)
        {
            //UpdatePeopleSection(___sections);
            DoOwnCaravanFormButtons(___sections, ref anythingChanged);
        }

        public static void DoWindowContents_Postfix(Dialog_Trade __instance, bool ___playerIsCaravan, List<Tradeable> ___cachedTradeables, Rect inRect)
        {
            var anythingChanged = false;
            DoOwnTradeButtons(___cachedTradeables, ___playerIsCaravan, ref anythingChanged);
            if (anythingChanged) Traverse.Create(__instance).Method("CountToTransferChanged").GetValue();
        }

        public static void Dialog_FormCaravan_Postfix(ref bool ___autoSelectTravelSupplies)
        {
            if (InitGC.autoSupplyDisabled) ___autoSelectTravelSupplies = false;
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

        public static bool smallLayoutCompatibility = false;
        private static void DoOwnCaravanFormButtons(List<Section> sections, ref bool anythingChanged)
        {
            GUI.BeginGroup(new Rect(350f, 0f, 530f, 27f));
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(0f, 0f, smallLayoutCompatibility ? 140f : 155f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "Select");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 10f, 0f, 40f, 27f);
            if (Widgets.ButtonText(rect2, "All", true, true, true))
            {
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

            if (Widgets.ButtonText(new Rect(rect2.xMax + 15f + 140f, 0f, 70f, 27f), "Goods2", true, true, true))
            {
                //FilterCombs.ApplyJourney(sections, caravanMembers);
                FilterCombs.ApplyGoods2(sections);
                anythingChanged = true;
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 20f + 210f, 0f, 50f, 27f), "Clear", true, true, true))
            {
                FilterCombs.ApplyNone(sections);
                anythingChanged = true;
            }
            Widgets.CheckboxLabeled(new Rect(rect2.xMax + 25f + 265f, 0f, smallLayoutCompatibility ? 60f : 77f, 30f), "Supply disabled", ref InitGC.autoSupplyDisabled);
            GUI.EndGroup();
        }

        private static void DoOwnTradeButtons(List<Tradeable> tradeables, bool playerIsCaravan, ref bool anythingChanged)
        {
            // on worldmap
            if (playerIsCaravan) GUI.BeginGroup(new Rect(550f, 52f, 460f, 27f));
            // on map
            else GUI.BeginGroup(new Rect(550f, 0f, 460f, 27f));

            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(0f, 0f, 30f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "Sell");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 5f, 0f, 40f, 27f);
            if (Widgets.ButtonText(rect2, "All", true, true, true))
            {
                FilterCombs.ApplyAllTrade(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 5f, 0f, 60f, 27f), "Goods", true, true, true))
            {
                FilterCombs.ApplyGoodsTrade(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f + 60f, 0f, 60f, 27f), "Goods2", true, true, true))
            {
                FilterCombs.ApplyGoodsTrade2(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 15f + 120f, 0f, 50f, 27f), "Reset", true, true, true))
            {
                FilterCombs.ApplyNoneTrade(tradeables);
                anythingChanged = true;
            }
            GUI.EndGroup();
        }


    }

}
