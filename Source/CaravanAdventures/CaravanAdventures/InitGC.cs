using System.Linq;
using RimWorld;
using Verse;

namespace CaravanAdventures
{
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
            if (!ModSettings.autoRemoveAbandondSettlementRuins) return;
            RemoveRuins();
            removeRuinsTick++;
        }

        private void RemoveRuins()
        {
            if (removeRuinsTick > 60000)
            {
                removeRuinsTick = 0;
                var settlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement != null && (settlement.def == WorldObjectDefOf.AbandonedSettlement || settlement.def == WorldObjectDefOf.GravshipLaunch || settlement.def == WorldObjectDefOf.AbandonedCamp) && settlement?.Faction?.IsPlayer == true);
                DLog.Message($"Trying to remove {settlements?.Count()} settlements");
                if (!(settlements?.Any() ?? false)) return; 

                foreach (var settlement in settlements.Reverse())
                {
                    Find.WorldObjects.Remove(settlement);
                }
            }
        }

    }
}
