using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Settings
{
    class SettingsCamp : Page
    {
        private float width;
        private Vector2 scrollPos;

        public SettingsCamp()
        {
            doCloseButton = true;
            closeOnCancel = true;
            scrollPos = Vector2.zero;

            width = 600f; 
        }

        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect((UI.screenWidth - width) / 2f, (UI.screenHeight - InitialSize.y) / 2f, width, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            Text.Font = GameFont.Medium;
            options.Label("Camp settings:".Colorize(Color.green), 40f);
            Text.Font = GameFont.Small;
            options.Gap();

            options.CheckboxLabeled($"Use custom camping map size", ref ModSettings.useCustomMapSize, "When disabled, the regular map size is being used");
            if (ModSettings.useCustomMapSize)
            {
                options.Label($"Select the mapsize for camps: {ModSettings.campMapSize.x}/1/{ModSettings.campMapSize.z}");
                var mapSize = Convert.ToInt32(options.Slider(ModSettings.campMapSize.x / 25, 3, 13) * 25);
                if (mapSize <= 75) mapSize = 250;
                ModSettings.campMapSize = new IntVec3(mapSize, 1, mapSize);
            }
            options.Gap();
            options.Label("Select the tent types you want ur pawns to build by themselfs.");
            options.CheckboxLabeled("Build production tent", ref ModSettings.hasProductionTent);
            options.CheckboxLabeled("Build storage tent", ref ModSettings.hasStorageTent);
            options.CheckboxLabeled("Build medical tent", ref ModSettings.hasMedicalTent);
            options.CheckboxLabeled("Build animal area", ref ModSettings.hasAnimalArea);
            options.CheckboxLabeled("Build prison tent", ref ModSettings.hasPrisonTent);
            options.CheckboxLabeled("Build plant tent", ref ModSettings.hasPlantTent);
            options.Gap();
            options.CheckboxLabeled("Auto apply camp gear recipes (e.g. food depending on pawn number, clothes)", ref ModSettings.autoApplyCampGearRecipes);
            options.CheckboxLabeled("Auto apply zone for snow to be cleared within the camp", ref ModSettings.autoApplyCampClearSnowArea);
            options.CheckboxLabeled("Generate storage for all inventory items", ref ModSettings.generateStorageForAllInventory, "When disabled, most items will remain packed on the animals");
            options.CheckboxLabeled("Let animals mostly free instead of limiting them to their small animal area", ref ModSettings.letAnimalsRunFree);
            options.CheckboxLabeled("Fence posts only decorative", ref ModSettings.decorativeFencePosts, "When set to false, fence posts can't be walked through and a door is created as entrace.");
            options.CheckboxLabeled("Show camp supply cost approximate on settle gizmo", ref ModSettings.showSupplyCostsInGizmo, "Enable if the approximate amount of camp supplies needed / available should be shown on the caravan settle gui gizmo label (value may be incorrect by a few units)");
            var rect = options.GetRect(Text.LineHeight);
            rect.width = options.ColumnWidth / 2;
            Widgets.Label(rect, "Maximum camp supply cost: ");
            var campCostInput = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), ModSettings.maxCampSupplyCost.ToString());
            if (double.TryParse(campCostInput, out var campCostDouble)) ModSettings.maxCampSupplyCost = Convert.ToInt32(campCostDouble);
            options.CheckboxLabeled("Disable Tentsupply requirement costs", ref ModSettings.hasSupplyCostsDisabled);
            options.Label($"Starting fuel percentage for camp-gear: {ModSettings.fuelStartingFillPercentage}%");
            ModSettings.fuelStartingFillPercentage = Convert.ToInt32(options.Slider(ModSettings.fuelStartingFillPercentage, 0, 100));
            options.End();
        }

        private IEnumerable<Widgets.DropdownMenuElement<Faction>> GenerateDropDownElements(List<Faction> factions)
        {
            foreach (var faction in factions)
            {
                if (faction.def.permanentEnemy || faction == Faction.OfPlayer || faction.leader == null || faction.def.techLevel < TechLevel.Industrial) continue;
                if (ModSettings.storyEnabled && faction != CaravanStory.StoryUtility.FactionOfSacrilegHunters) continue;
                yield return new Widgets.DropdownMenuElement<Faction>() { option = new FloatMenuOption(faction.Name, () => CompCache.BountyWC.BountyFaction = faction), payload = faction };
            }
        }
    }
}
