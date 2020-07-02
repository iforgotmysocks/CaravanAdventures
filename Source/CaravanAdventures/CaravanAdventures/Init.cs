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

        public Init(World world) : base(world)
        {

        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FilterCombs.InitFilterSets();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false);
        }


    }
}
