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

            options.Label($"Select the mapsize for camps: {ModSettings.campMapSize.x}/1/{ModSettings.campMapSize.z}");
            var mapSize = Convert.ToInt32(options.Slider(ModSettings.campMapSize.x / 25, 3, 12) * 25);
            if (mapSize <= 75) mapSize = 275;
            ModSettings.campMapSize = new IntVec3(mapSize, 1, mapSize);

            options.Gap();
            options.Label("Select the tent types you want ur pawns to build by themselfs.");
            options.Gap();

            options.CheckboxLabeled("Build production tent", ref ModSettings.hasProductionTent);
            options.CheckboxLabeled("Build storage tent", ref ModSettings.hasStorageTent);
            options.CheckboxLabeled("Build medical tent", ref ModSettings.hasMedicalTent);
            options.CheckboxLabeled("Build animal area", ref ModSettings.hasAnimalArea);
            options.CheckboxLabeled("Build prison tent", ref ModSettings.hasPrisonTent);
            options.CheckboxLabeled("Build plant tent", ref ModSettings.hasPlantTent);

            options.CheckboxLabeled("Generate storage for all inventory items", ref ModSettings.generateStorageForAllInventory, "When disabled, most items will remain packed on the animals");
            options.CheckboxLabeled("Let animals mostly free instead of limiting them to their small animal area", ref ModSettings.letAnimalsRunFree);

            options.CheckboxLabeled("Disable Tentsupply requirement costs", ref ModSettings.hasSupplyCostsDisabled);
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
