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
        private static bool pawnFlag = false;
        private static bool thingFlag = false;

        public static void ApplyPatches()
        {
            if (!ModSettings.caravanFormingFilterSelectionEnabled) return;
            var orgOnGUI = AccessTools.Method(typeof(TransferableOneWayWidget), "OnGUI", new Type[]
            {
                typeof(Rect),
                typeof(bool).MakeByRefType()
            });
            var postOnGUI = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(OnGUI_Postfix)));
            HarmonyPatcher.harmony.Patch(orgOnGUI, null, postOnGUI);

            var orgDoWindowContents = AccessTools.Method(typeof(Dialog_Trade), "DoWindowContents", new Type[]
            {
                typeof(Rect),
            });
            var postDoWindowContents = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(DoWindowContents_Postfix)));
            HarmonyPatcher.harmony.Patch(orgDoWindowContents, null, postDoWindowContents);

            var orgDialog_FormCaravan = AccessTools.DeclaredConstructor(typeof(Dialog_FormCaravan), new[] {
                typeof(Map),
                typeof(bool),
                typeof(Action),
                typeof(bool)
            });
            var postDialog_FormCaravan = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(Dialog_FormCaravan_Postfix)));
            HarmonyPatcher.harmony.Patch(orgDialog_FormCaravan, null, postDialog_FormCaravan);

            var orgDialog_FormCaravan_PostOpen = AccessTools.Method(typeof(Dialog_FormCaravan), "PostOpen");
            var postDialog_FormCaravan_PostOpen = new HarmonyMethod(typeof(AutomaticItemSelection).GetMethod(nameof(Dialog_FormCaravan_PostOpen_Postfix)));
            HarmonyPatcher.harmony.Patch(orgDialog_FormCaravan_PostOpen, null, postDialog_FormCaravan_PostOpen);
        }

        public static void OnGUI_Postfix(TransferableOneWayWidget __instance, List<Section> ___sections, List<Section> __state, Rect inRect, ref bool anythingChanged)
        {
            DoOwnCaravanFormButtons(___sections, ref anythingChanged);
        }

        // currently disabled -> using PostOpen
        private static void SelectPawns(List<Section> sections, ref bool anythingChanged)
        {
            if (pawnFlag == false && ModSettings.autoSelectPawns)
            {
                pawnFlag = true;
                if (caravanMembers == null)
                {
                    DLog.Message($"caravanMembers null");
                    return;
                }

                foreach (var section in sections)
                {
                    foreach (var trans in section.transferables)
                    {
                        var pawn = (Pawn)trans?.AnyThing;
                        if (pawn == null) continue;
                        if (caravanMembers.Contains(pawn) && Find.Selector.SelectedPawns.Contains(pawn))
                        {
                            FilterHelper.SetMaxAmount(trans);
                            anythingChanged = true;
                        }
                    }
                }
            }
        }

        public static void DoWindowContents_Postfix(Dialog_Trade __instance, bool ___playerIsCaravan, List<Tradeable> ___cachedTradeables, Rect inRect)
        {
            var anythingChanged = false;
            DoOwnTradeButtons(___cachedTradeables, ___playerIsCaravan, ref anythingChanged);
            if (anythingChanged) Traverse.Create(__instance).Method("CountToTransferChanged").GetValue();
        }

        public static void Dialog_FormCaravan_Postfix(Dialog_FormCaravan __instance, ref bool ___autoSelectFoodAndMedicine, Map ___map)
        {
            if (InitGC.autoSupplyDisabled) ___autoSelectFoodAndMedicine = false;
            thingFlag = false;
            //if (ModSettings.autoSelectPawns) caravanMembers = RimWorld.Planet.CaravanFormingUtility.AllSendablePawns(___map);
        }

        public static void Dialog_FormCaravan_PostOpen_Postfix(Dialog_FormCaravan __instance)
        {
            if (thingFlag) return;
            var anythingChanged = false;
            if (ModSettings.autoSelectItems || ModSettings.autoSelectPawns) SelectThings(__instance.transferables, ref anythingChanged);
            if (anythingChanged) Traverse.Create(__instance).Method("CountToTransferChanged").GetValue();
            thingFlag = true;
        }

        private static void SelectThings(List<TransferableOneWay> transferables, ref bool anythingChanged)
        {
            foreach (var trans in transferables)
            {
                var thing = trans?.AnyThing;
                if (thing == null) continue;
                var selected = Find.Selector.SelectedObjects.Where(x => x != null && trans.things.Contains(x))?.OfType<Thing>();
                if (selected == null || selected.Count() == 0) continue;

                var isPawn = thing is Pawn;
                if (!ModSettings.autoSelectPawns && isPawn) continue;
                else if (!ModSettings.autoSelectItems && !isPawn) continue;

                FilterHelper.SetAmount(trans, selected.Select(x => x.stackCount).Sum());
                anythingChanged = true;
            }
        }

        private static void UpdatePeopleSection(List<Section> ___sections)
        {
            var detectedPeople = ___sections.SelectMany(section => section.cachedTransferables.Where(trans =>
            {
                var pawn = trans?.AnyThing as Pawn;
                if (pawn == null) return false;
                if (pawn.RaceProps?.Humanlike == true && trans.CountToTransfer > 0) return true;
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
            Rect rect = new Rect(0f, 0f, smallLayoutCompatibility ? 40f : 55f, 27f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "CAFormingTradePresetSelect".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 10f, 0f, 40f, 27f);
            if (Widgets.ButtonText(rect2, "CAFormingTradePresetAll".Translate(), true, true, true))
            {
                FilterCombs.ApplyAll(sections);
                anythingChanged = true;
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 5f, 0f, 65f, 27f), "CAFormingTradePresetPackUp".Translate(), true, true, true))
            {
                FilterCombs.ApplyPackUp(sections);
                anythingChanged = true;
            }
           
            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f + 65f, 0f, 65f, 27f), "CAFormingTradePresetGoods".Translate(), true, true, true))
            {
                FilterCombs.ApplyGoods(sections);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 15f + 130f, 0f, 65f, 27f), "CAFormingTradePresetGoods2".Translate(), true, true, true))
            {
                //FilterCombs.ApplyJourney(sections, caravanMembers);
                FilterCombs.ApplyGoods2(sections);
                anythingChanged = true;
            }

            if (Widgets.ButtonImageWithBG(new Rect(rect2.xMax + 20f + 195f, 0f, 27f, 27f), TexCustom.CaravanSettings, new Vector2(21, 21))) 
            {
                Find.WindowStack.Add(new Settings.SettingsFilters());
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 25f + 222f, 0f, 45f, 27f), "CAFormingTradePresetClear".Translate(), true, true, true))
            {
                FilterCombs.ApplyNone(sections);
                anythingChanged = true;
            }
            Widgets.CheckboxLabeled(new Rect(rect2.xMax + 30f + 267f, 0f, 77f, 30f), "CAFormingTradePresetSupply".Translate(), ref InitGC.autoSupplyDisabled);
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
            Widgets.Label(rect, "CAFormingTradePresetSell".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(rect.xMax + 5f, 0f, 40f, 27f);
            if (Widgets.ButtonText(rect2, "CAFormingTradePresetAll".Translate(), true, true, true))
            {
                FilterCombs.ApplyAllTrade(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 5f, 0f, 60f, 27f), "CAFormingTradePresetGoods".Translate(), true, true, true))
            {
                FilterCombs.ApplyGoodsTrade(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f + 60f, 0f, 60f, 27f), "CAFormingTradePresetGoods2".Translate(), true, true, true))
            {
                FilterCombs.ApplyGoodsTrade2(tradeables);
                anythingChanged = true;
            }

            if (Widgets.ButtonImageWithBG(new Rect(rect2.xMax + 15f + 120f, 0f, 27f, 27f), TexCustom.CaravanSettings, new Vector2(21, 21)))
            {
                Find.WindowStack.Add(new Settings.SettingsFilters());
            }

            if (Widgets.ButtonText(new Rect(rect2.xMax + 20f + 147f, 0f, 50f, 27f), "CAFormingTradePresetClear".Translate(), true, true, true))
            {
                FilterCombs.ApplyNoneTrade(tradeables);
                anythingChanged = true;
            }
            GUI.EndGroup();
        }


    }

}
