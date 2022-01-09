using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using CaravanAdventures.CaravanCamp;

namespace CaravanAdventures.CaravanImprovements
{
    public class CompCaravanDecisions : WorldObjectComp
    {
        public bool unpackGearOnSettling = true;
        public bool allowNightTravel = false;
        private static StringBuilder tmpSettleFailReason = new StringBuilder();
        private int campCost = -1;
        private int suppliesAvailable = -1;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref unpackGearOnSettling, "unpackGearOnSettling", true);
            Scribe_Values.Look(ref allowNightTravel, "allowNightTravel", false);
        }

        private void UpdateCampCostAndSupplies(Caravan caravan)
        {
            campCost = CampBuilder.PreemptivelyCalculateCampCosts(caravan);
            suppliesAvailable = caravan.AllThings?.Where(thing => thing.def == CampDefOf.CASpacerTentSupplies)?.Select(thing => thing?.stackCount)?.Sum() ?? 0;
        }

        private static void SettleWithoutDroppingGear(Caravan caravan, bool createCamp = false)
        {
            Faction faction = caravan.Faction;
            if (faction != Faction.OfPlayer)
            {
                Log.Error("Cannot settle with non-player faction.", false);
                return;
            }
            Settlement newHome = SettleUtility.AddNewHome(caravan.Tile, faction);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                var map = GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, ModSettings.useCustomMapSize ? ModSettings.campMapSize : Find.World.info.initialMapSize, null);
                if (createCamp) CampBuilderHook(caravan, map).GenerateCamp();
            }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = newHome.Map;
                var unloadComp = map.GetComponent<CompUnloadItems>();
                if (unloadComp != null && CaravanInventoryUtility.AllInventoryItems(caravan).Count != 0) unloadComp.Unload = true;
                Thing t = caravan.PawnsListForReading[0];
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, false, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
                CameraJumper.TryJump(t);
            }, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
        }

        private static CampBuilder CampBuilderHook(Caravan caravan, Map map)
        {
            return new CampBuilder(caravan, map);
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (Find.WorldSelector.SingleSelectedObject == this.parent && this.parent != null && this.parent.Faction != null && this.parent.Faction == Faction.OfPlayerSilentFail)
            {
                var cmdSettleAsCaravan = new Command_Settle
                {
                    defaultLabel = "SettleWithoutUnloading".Translate(),
                    defaultDesc = "SettleWithoutUnloadingDesc".Translate(),
                    order = 198f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle/SettleWithoutUnloading_up", true),
                    action = () => {
                        Action settleAction = () => SettleWithoutDroppingGear(caravan);
                        SettlementProximityGoodwillUtility.CheckConfirmSettle(caravan.Tile, settleAction);
                    }
                };
                tmpSettleFailReason.Length = 0;
                if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
                {
                    cmdSettleAsCaravan.Disable(tmpSettleFailReason.ToString());
                }
                yield return cmdSettleAsCaravan;

                if (ModSettings.showSupplyCostsInGizmo && (Find.TickManager.TicksGame % 61 == 0 || campCost == -1) && caravan != null) UpdateCampCostAndSupplies(caravan);

                var cmdSettleWithCamp = new Command_Settle
                {
                    defaultLabel = "SettleWithCampLabel".Translate() + (ModSettings.showSupplyCostsInGizmo ? $" ({suppliesAvailable}/{campCost})" : ""),
                    defaultDesc = "SettleWithCampDesc".Translate(),
                    order = 198f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle/SettleCamp_up", true),
                    action = () => {
                        Action settleAction = () => SettleWithoutDroppingGear(caravan, true);
                        if (!ModSettings.caravanCampProximityRemoval) SettlementProximityGoodwillUtility.CheckConfirmSettle(caravan.Tile, settleAction);
                        else settleAction();
                    }
                };
                tmpSettleFailReason.Length = 0;
                if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
                {
                    cmdSettleWithCamp.Disable(tmpSettleFailReason.ToString());
                }
                yield return cmdSettleWithCamp;

                var cmdAllowNightTravel = new Command_Toggle
                {
                    isActive = () => allowNightTravel,
                    toggleAction = () => allowNightTravel = !allowNightTravel,
                    defaultLabel = "AllowNightTravel".Translate(),
                    defaultDesc = "AllowNightTravelDesc".Translate(),
                    order = 199f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle/Nighttravel", true),
                };
                yield return cmdAllowNightTravel;
            }
            yield break;
        }


    }
}
