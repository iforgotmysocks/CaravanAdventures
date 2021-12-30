using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CaravanAdventures.CaravanCamp
{
    public class CompCampControl : ThingComp
    {
        protected List<CellRect> campRects;
        protected int resourceCount;
        protected int waste;
        protected bool tribal;
        protected List<Thing> campAssets;

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref campRects, "campRects", LookMode.Value);
            Scribe_Values.Look(ref resourceCount, "resourceCount", 0);
            Scribe_Values.Look(ref tribal, "tribal", true);
            Scribe_Collections.Look(ref campAssets, "campAssets", LookMode.Reference);
            Scribe_Values.Look(ref waste, "waste", 0);
        }

        public List<CellRect> CampRects { get => campRects; set => campRects = value; }
        public int ResourceCount { get => resourceCount; set => resourceCount = value; }
        public bool Tribal { get => tribal; set => tribal = value; }
        public List<Thing> CampAssets { get => campAssets; set => campAssets = value; }
        public int Waste { get => waste; set => waste = value; }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var baseOption in base.CompFloatMenuOptions(selPawn)) yield return baseOption;

            yield return new FloatMenuOption("CADeconstructCamp".Translate(), () =>
            {
                Messages.Message(new Message("CADeconstructCampMessage".Translate(), MessageTypeDefOf.NeutralEvent));

                var job = JobMaker.MakeJob(CampDefOf.CACampInformPackingUp, parent);
                job.count = 1;
                DLog.Message($"trying to take ordered");
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });

            yield return new FloatMenuOption("CALeaveImmediately".Translate(), () =>
            {
                //Messages.Message(new Message($"Leaving immediately", MessageTypeDefOf.NeutralEvent));

                var tile = parent.Map.Tile;
                Find.WindowStack.Add(new Dialog_FormCaravan(parent.Map, true, () =>
                {
                    if (parent.Map.mapPawns.AnyColonistSpawned) return;
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation($"{(tribal ? "CALeaveImmediatelyPackupCampTribal" : "CALeaveImmediatelyPackupCamp")}".Translate(Convert.ToInt32(resourceCount * 0.7)), delegate
                    {
                        var mp = parent.Map.Parent;
                        Current.Game.DeinitAndRemoveMap(parent.Map);
                        mp.Destroy();

                        if (tribal) return;
                        var caravan = Find.WorldObjects.Caravans.FirstOrDefault(x => x.IsPlayerControlled && x.Tile == tile);
                        if (caravan == null)
                        {
                            DLog.Warning($"caravan not found, couldn't add campgear");
                            return;
                        }
                        var supplies = ThingMaker.MakeThing(CampDefOf.CASpacerTentSupplies);
                        supplies.stackCount = Convert.ToInt32(resourceCount * 0.7);
                        if (supplies.stackCount >= 1) caravan.AddPawnOrItem(supplies, false);
                    }, false, null));
                }, false));
            });

        }

        private void ForceReform(MapParent mapParent)
        {
            var tmpPawns = new List<Pawn>();
            if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, true).Any((Pawn x) => x.IsColonist))
            {
                Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent, true);
                Current.Game.CurrentMap = mapParent.Map;
                Dialog_FormCaravan window = new Dialog_FormCaravan(mapParent.Map, true, delegate ()
                {
                    if (mapParent.HasMap)
                    {
                        mapParent.Destroy();
                    }
                }, false);
                Find.WindowStack.Add(window);
                return;
            }
            tmpPawns.Clear();
            tmpPawns.AddRange(from x in mapParent.Map.mapPawns.AllPawns
                              where x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer
                              select x);
            if (tmpPawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
            {
                CaravanExitMapUtility.ExitMapAndCreateCaravan(tmpPawns, Faction.OfPlayer, mapParent.Tile, mapParent.Tile, -1, true);
            }
            tmpPawns.Clear();
            mapParent.Destroy();
        }

        public bool PackUpTentAtRandomRect()
        {
            if (CampRects.Count == 1) return false;
            var selRect = CampRects.Where(rect => !rect.Cells.Contains(parent.Position)).RandomElement();
            PackUpTent(selRect);
            CampRects.Remove(selRect);
            return true;
        }

        public void FinishPackingReainingTentsAndControl()
        {
            foreach (var rect in CampRects) PackUpTent(rect);

            if (!tribal && resourceCount != 0)
            {
                var count = Convert.ToInt32(Math.Ceiling((double)CampDefOf.CASpacerTentSupplies.stackLimit / (double)resourceCount));
                var remaining = resourceCount - waste;
                DLog.Message($"Refunding {remaining} of {ResourceCount} resources");
                for (var i = 0; i < count; i++)
                {
                    var thing = ThingMaker.MakeThing(CampDefOf.CASpacerTentSupplies);
                    var substact = Math.Min(remaining, thing.def.stackLimit);
                    remaining -= substact;
                    thing.stackCount = substact;
                    GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
            }

            RemoveControl();
        }

        private void PackUpTent(CellRect rect)
        {
            try
            {
                var prison = rect.Cells.Any(cell => cell.GetFirstBuilding(parent.Map) is Building_Bed bed && bed.ForPrisoners) && rect.Cells.Any(cell => { var pawn = cell.GetFirstPawn(parent.Map); return pawn != null && pawn.IsPrisoner; });

                foreach (var cell in rect)
                {
                    if (parent.Map.roofGrid.Roofed(cell)) parent.Map.roofGrid.SetRoof(cell, null);
                    if (parent.Map.terrainGrid.TerrainAt(cell) == CampDefOf.CATentFloor
                        || parent.Map.terrainGrid.TerrainAt(cell) == CampDefOf.CAMakeshiftTentFloor)
                        parent.Map.terrainGrid.RemoveTopLayer(cell);

                    var assetsAtCell = campAssets.Where(asset => asset?.Position == cell);
                    foreach (var asset in assetsAtCell.Reverse<Thing>())
                    {
                        if (asset == null || asset == parent || asset.Destroyed) continue;

                        if (asset is Building_Storage storageBuilding)
                        {
                            var storedThings = storageBuilding.AllSlotCellsList().Where(slot => slot.GetFirstItem(parent.Map) != null)?.Select(slot => slot.GetFirstItem(parent.Map));
                            if (storedThings != null && storedThings.Count() != 0)
                            {
                                foreach (var thing in storedThings.Reverse())
                                {
                                    var pos = new IntVec3(thing.Position.x - 1, thing.Position.y, thing.Position.z);
                                    thing.DeSpawn();
                                    GenPlace.TryPlaceThing(thing, pos, parent.Map, ThingPlaceMode.Near, out var result);
                                }
                            }
                        }

                        if (prison && (asset.def == CampDefOf.CATentWall || asset.def == CampDefOf.CATentDoor && asset.Stuff == CampDefOf.CASpacerTentFabric))
                        {
                            asset.Destroy();
                            var wall = asset.def == CampDefOf.CATentDoor ? ThingMaker.MakeThing(CampDefOf.CATentDoor, CampDefOf.CAMakeshiftTentLeather) : ThingMaker.MakeThing(CampDefOf.CAMakeshiftTentWall, CampDefOf.CAMakeshiftTentLeather);
                            wall.SetFaction(Faction.OfPlayer);
                            GenSpawn.Spawn(wall, asset.Position, parent.Map);
                            continue;
                        }
                        else if (prison && (asset.def == CampDefOf.CAMakeshiftTentWall || asset.def == CampDefOf.CATentDoor)) continue;
                        asset.Destroy();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public void RemoveControl() => this.parent.Destroy();


    }
}
