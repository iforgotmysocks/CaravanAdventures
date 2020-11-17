using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CaravanAdventures.CaravanCamp
{
    class CompCampControl : ThingComp
    {
        // todo save tents
        // check for building that can be used as middle "control" thingy
        // turn MapComp into a thingcomp for a thing that's a light and can't be destroyed
        // apply 

        private List<CellRect> campRects;
        private int resourceCount;
        private int waste;
        private bool tribal;
        private List<Thing> campAssets;

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
                // todo start and move to destruction job
                // todo destroy everything but only stuff that was spawned and regain resources
                // best create a list of all thingdefs belonging to the camp and kill those
                Messages.Message(new Message($"destroying camp", MessageTypeDefOf.NeutralEvent));

                var job = JobMaker.MakeJob(CampDefOf.CACampInformPackingUp, parent);
                job.count = 1;
                Log.Message($"trying to take ordered");
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
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
                var count = Convert.ToInt32(Math.Ceiling((double)CampThingDefOf.CASpacerTentSupplies.stackLimit / (double)resourceCount));
                var remaining = resourceCount - waste;
                Log.Message($"Refunding {remaining} of {ResourceCount} resources");
                for (var i = 0; i < count; i++)
                {
                    var thing = ThingMaker.MakeThing(CampThingDefOf.CASpacerTentSupplies);
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
            foreach (var cell in rect)
            {
                if (parent.Map.roofGrid.Roofed(cell)) parent.Map.roofGrid.SetRoof(cell, null);
                if (parent.Map.terrainGrid.TerrainAt(cell) == CampDefOf.CATentFloor) parent.Map.terrainGrid.RemoveTopLayer(cell);

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
                    asset.Destroy();
                }
            }
        }

        public void RemoveControl() => this.parent.Destroy();


    }
}
