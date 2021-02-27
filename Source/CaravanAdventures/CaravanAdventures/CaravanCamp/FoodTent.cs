﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class FoodTent : Tent, IZoneTent
    {
        private Zone_Stockpile zone;
        private ThingCategoryDef[] validFoods = new[] { ThingCategoryDefOf.FoodMeals, ThingCategoryDefOf.Foods, ThingCategoryDefOf.MeatRaw };
        private ThingDef[] unvalidFoods = new[] { ThingDefOf.Kibble, ThingDefOf.Hay };
        public FoodTent()
        {
            this.CoordSize = 2;
            SupplyCost = 3;
        }

        public Zone GetZone() => zone;
        public override void Build(Map map, List<Thing> campAssetListRef)
        {
            base.Build(map, campAssetListRef);
            var cacoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            var cacooler = CampHelper.PrepAndGenerateThing(CampDefOf.CACooler, cacoolerPos, map, default, campAssetListRef);
            CampHelper.RefuelByPerc(cacooler, -1);
        }

        public override void BuildTribal(Map map, List<Thing> campAssetListRef)
        {
            base.BuildTribal(map, campAssetListRef);

            // todo tribal cooler solution?
            //var cacoolerPos = CellRect.Cells.FirstOrDefault(cell => cell.x == CellRect.maxX - 1 && cell.z == CellRect.minZ + 1);
            //var cacooler = GenSpawn.Spawn(CampThingDefOf.CACooler, cacoolerPos, map);
            //campAssetListRef.Add(cacooler);
            //cacooler.SetFaction(Faction.OfPlayer);
            //var refuelComp = cacooler.TryGetComp<CompRefuelable>();
            //if (refuelComp != null) refuelComp.Refuel(refuelComp.GetFuelCountToFullyRefuel());
        }

        public virtual void CreateZone(Map map)
        {
            zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            zone.settings.filter = new ThingFilter();
            zone.settings.filter.SetAllow(SpecialThingFilterDef.Named("AllowRotten"), false);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.Foods, true);
            zone.settings.filter.SetAllow(ThingDefOf.Kibble, false);
            zone.settings.filter.SetAllow(ThingDefOf.Hay, false);
            zone.settings.filter.SetAllow(ThingDef.Named("Meat_Megaspider"), false);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, true);
            zone.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesInsect, false);
            zone.settings.Priority = StoragePriority.Preferred;
            zone.label = "CAFoodZoneLabel".Translate();
            CellRect.Cells.Where(cell => cell != null && !CellRect.EdgeCells.Contains(cell)).ToList().ForEach(cell => zone.AddCell(cell));
            //zone.CheckContiguous();
        }

        public void ApplyInventory(Map map, Caravan caravan)
        {
            foreach (var cell in zone.Cells)
            {
                var stack = CampHelper.GetFirstOrderedThingOfCategoryFromCaravan(caravan, validFoods, unvalidFoods);
                if (stack == null) break;
                if (!cell.Filled(map)) GenDrop.TryDropSpawn_NewTmp(stack, cell, map, ThingPlaceMode.Direct, out var result);
            }
        }

    }
}
