using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanImprovements
{
    class CompUnloadItems : MapComponent
    {
        private bool unload = false;
        public bool Unload { get; set; }
        public CompUnloadItems(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref unload, "unload");
        }

    }
}
