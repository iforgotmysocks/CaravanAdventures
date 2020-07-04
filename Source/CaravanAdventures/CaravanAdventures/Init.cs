using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using CaravanAdventures.CaravanItemSelection;

namespace CaravanAdventures
{
    class Init : WorldComponent
    {
        private int removeRuinsTick = 0;
        public Init(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FilterCombs.InitFilterSets();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            RemoveRuins();


            removeRuinsTick++;
        }

        private void RemoveRuins()
        {
            if (removeRuinsTick > 60000)
            {
                var settlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement.def == WorldObjectDefOf.AbandonedSettlement && settlement.Faction.IsPlayer);
                Log.Message($"Trying to remove {settlements.Count()} settlements");

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
