using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void UpdateCampCostAndSupplies(Caravan caravan)
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
                // Find.World.info.initialMapSize
                var map = GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, ModSettings.campMapSize, null);
                if (createCamp) new CampBuilder(caravan, map).GenerateCamp();
            }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = newHome.Map;
                var unloadComp = map.GetComponent<CompUnloadItems>();
                if (unloadComp != null && CaravanInventoryUtility.AllInventoryItems(caravan).Count != 0) unloadComp.Unload = true;
                Thing t = caravan.PawnsListForReading[0];
                // todo use own utility once the new gizmo for dropping items is in
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, false, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
                CameraJumper.TryJump(t);
            }, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
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
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle", true),
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
                else if (SettleUtility.PlayerSettlementsCountLimitReached)
                {
                    if (Prefs.MaxNumberOfPlayerSettlements > 1) cmdSettleAsCaravan.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
                    else cmdSettleAsCaravan.Disable("CommandSettleFailAlreadyHaveBase".Translate());
                }
                yield return cmdSettleAsCaravan;

                if (ModSettings.showSupplyCostsInGizmo && (Find.TickManager.TicksGame % 61 == 0 || campCost == -1) && caravan != null) UpdateCampCostAndSupplies(caravan);

                var cmdSettleWithCamp = new Command_Settle
                {
                    defaultLabel = "SettleWithCampLabel".Translate() + (ModSettings.showSupplyCostsInGizmo ? $" ({campCost}/{suppliesAvailable})" : ""),
                    defaultDesc = "SettleWithCampDesc".Translate(),
                    order = 198f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle", true),
                    action = () => {
                        Action settleAction = () => SettleWithoutDroppingGear(caravan, true);
                        SettlementProximityGoodwillUtility.CheckConfirmSettle(caravan.Tile, settleAction);
                    }
                };
                tmpSettleFailReason.Length = 0;
                if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
                {
                    cmdSettleWithCamp.Disable(tmpSettleFailReason.ToString());
                }
                else if (SettleUtility.PlayerSettlementsCountLimitReached)
                {
                    if (Prefs.MaxNumberOfPlayerSettlements > 1) cmdSettleWithCamp.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
                    else cmdSettleWithCamp.Disable("CommandSettleFailAlreadyHaveBase".Translate());
                }
                yield return cmdSettleWithCamp;

                var cmdAllowNightTravel = new Command_Toggle
                {
                    isActive = () => allowNightTravel,
                    toggleAction = () => allowNightTravel = !allowNightTravel,
                    defaultLabel = "AllowNightTravel".Translate(),
                    defaultDesc = "AllowNightTravelDesc".Translate(),
                    order = 199f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle", true),
                };
                yield return cmdAllowNightTravel;
            }
            yield break;
        }


    }
}
