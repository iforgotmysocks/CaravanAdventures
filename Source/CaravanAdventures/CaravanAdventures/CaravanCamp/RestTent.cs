using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class RestTent : Tent
    {
        public List<Pawn> Occupants { get; set; }

        public RestTent()
        {
            Occupants = new List<Pawn>();
        }

        public override void Build(Map map)
        {
            base.Build(map);
            var lover = Occupants.FirstOrDefault(col => col != null && LovePartnerRelationUtility.ExistingLovePartner(col) != null
                && Occupants.Contains(LovePartnerRelationUtility.ExistingLovePartner(col)));
            var otherLover = lover != null ? LovePartnerRelationUtility.ExistingLovePartner(lover) : null;
            var cellSpots = CellRect.Cells.Where(cell => !CellRect.EdgeCells.Contains(cell) && cell.z == CellRect.maxZ - 1).ToArray();

            for (int i = 0; i < cellSpots.Length; i++)
            {
                if (lover != null && i == 0) continue;
                else if (lover != null && i == 1)
                {
                    var dbThing = ThingMaker.MakeThing(ThingDef.Named("BedrollDouble"), ThingDefOf.Cloth);
                    var doubleBed = GenSpawn.Spawn(dbThing, cellSpots[i], map, Rot4.South);
                    doubleBed.SetFaction(Faction.OfPlayer);
                    lover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                    otherLover.ownership.ClaimBedIfNonMedical((Building_Bed)doubleBed);
                }
                else
                {
                    var thing = ThingMaker.MakeThing(ThingDefOf.Bedroll, ThingDefOf.Cloth);
                    var bed = GenSpawn.Spawn(thing, cellSpots[i], map, Rot4.South);
                    bed.SetFaction(Faction.OfPlayer);
                    var pawnInNeedOfBed = Occupants.FirstOrDefault(occ => occ != null && occ != lover && occ != otherLover && occ.ownership.OwnedBed == null);
                    if (pawnInNeedOfBed != null) pawnInNeedOfBed.ownership.ClaimBedIfNonMedical((Building_Bed)bed);
                }
            }

            var caheaterPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var caheater = GenSpawn.Spawn(CampThingDefOf.CAAirConditioningHeater, caheaterPos, map);
            caheater.SetFaction(Faction.OfPlayer);
            var refuelComp = caheater.TryGetComp<CompRefuelable>();
            // todo check if caravan has fuel
            if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());
        }
    }
}
