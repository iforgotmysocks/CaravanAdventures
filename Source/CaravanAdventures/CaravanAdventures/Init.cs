using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CaravanAdventures
{
    class Init : MapComponent
    {
        private bool chosenPawnSelected;

        public Init(Map map) : base(map)
        {

            // ModLister.RoyaltyInstalled or ModsConfig.RoyaltyActive

            // todo figure out why chosenPawnSelected isn't being saved!
            if (!chosenPawnSelected)
            {
                
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref chosenPawnSelected, "chosenPawnSelected", false);
        }


    }
}
