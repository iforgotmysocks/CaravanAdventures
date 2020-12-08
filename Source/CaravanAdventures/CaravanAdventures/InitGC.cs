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

        public InitGC(Game game)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removeRuinsTick, "removeRuinsTick", 0);
        }


    }
}
