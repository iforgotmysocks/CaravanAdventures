using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace CaravanAdventures.CaravanImprovements
{
    public class CompCaravanDecisions : WorldObjectComp
    {
        public bool unpackGearOnSettling = true;
        public bool allowNightTravel = false;
        private static StringBuilder tmpSettleFailReason = new StringBuilder();

        public static void SettleWithoutDroppingGear(Caravan caravan)
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
                GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, Find.World.info.initialMapSize, null);
            }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map map = newHome.Map;
                Thing t = caravan.PawnsListForReading[0];
                // todo use own utility once the new gizmo for dropping items is in
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, false, (IntVec3 x) => x.GetRoom(map, RegionType.Set_Passable).CellCount >= 600);
                CameraJumper.TryJump(t);
            }, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            var gizmos = base.GetCaravanGizmos(caravan).ToList();
            // todo check and change
            if (this.parent != null && this.parent.Faction != null && this.parent.Faction == Faction.OfPlayerSilentFail || true)
            {
                var settleCommand = new Command_Settle
                {
                    defaultLabel = "SettleWithoutUnloading".Translate(),
                    defaultDesc = "SettleWithoutUnloadingDesc".Translate(),
                    order = 198f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle", true),
                    action = (() => {
                        Action settleAction = (delegate () {
                            SettleWithoutDroppingGear(caravan);
                        });
                        SettlementProximityGoodwillUtility.CheckConfirmSettle(caravan.Tile, settleAction);
                    })
                };
                tmpSettleFailReason.Length = 0;
                if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
                {
                    settleCommand.Disable(tmpSettleFailReason.ToString());
                }
                else if (SettleUtility.PlayerSettlementsCountLimitReached)
                {
                    if (Prefs.MaxNumberOfPlayerSettlements > 1)
                    {
                        settleCommand.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
                    }
                    else
                    {
                        settleCommand.Disable("CommandSettleFailAlreadyHaveBase".Translate());
                    }
                }

                gizmos.Add(settleCommand);

                var command = new Command_Toggle
                {
                    defaultLabel = "AllowNightTravel".Translate(),
                    defaultDesc = "AllowNightTravelDesc".Translate(),
                    order = 199f,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Settle", true),
                    isActive = (() => this.allowNightTravel),
                    toggleAction = (() => this.allowNightTravel = !this.allowNightTravel)
                };
                gizmos.Add(command);
            }
            return gizmos;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref unpackGearOnSettling, "unpackGearOnSettling", true);
            Scribe_Values.Look(ref allowNightTravel, "allowNightTravel", false);
        }

    }
}
