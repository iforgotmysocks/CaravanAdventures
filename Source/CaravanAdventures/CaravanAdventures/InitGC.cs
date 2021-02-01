using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using CaravanAdventures.CaravanItemSelection;
using CaravanAdventures.CaravanStory;

namespace CaravanAdventures
{
    // todo https://fluffy-mods.github.io//2020/08/13/debugging-rimworld/
    class InitGC : GameComponent
    {
        private int removeRuinsTick = 0;

        public static ThingFilter packUpFilter;
        public static bool packUpExclusive = true;

        public static ThingFilter goodsFilter;
        public static bool goodsExclusive = false;

        public static ThingFilter journeyFilter;
        public static bool journeyExclusive = false;

        public static bool autoSupplyDisabled = false;

        public InitGC(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removeRuinsTick, "removeRuinsTick", 0);
            Scribe_Deep.Look(ref packUpFilter, "packUpFilter");
            Scribe_Values.Look(ref packUpExclusive, "packUpExclusive", true);

            Scribe_Deep.Look(ref goodsFilter, "goodsFilter");
            Scribe_Values.Look(ref goodsExclusive, "goodsExclusive", false);

            Scribe_Deep.Look(ref journeyFilter, "journeyFilter");
            Scribe_Values.Look(ref journeyExclusive, "journeyExclusive", false);

            Scribe_Values.Look(ref autoSupplyDisabled, "autoSupplyDisabled", false);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CompCache.InitGC = null;
            InitFilters();
        }

        private void InitFilters()
        {
            if (packUpFilter == null || goodsFilter == null || journeyFilter == null) Settings.SettingsFilters.RestoreFilterDefaults();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            RemoveRuins();
            removeRuinsTick++;
        }

        private void RemoveRuins()
        {
            if (removeRuinsTick > 60000)
            {
                var settlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement.def == WorldObjectDefOf.AbandonedSettlement && settlement.Faction.IsPlayer);
                DLog.Message($"Trying to remove {settlements.Count()} settlements");

                foreach (var settlement in settlements.Reverse())
                {
                    Find.WorldObjects.Remove(settlement);
                }
                removeRuinsTick = 0;
            }
        }

    }
}
