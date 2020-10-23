using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class CompCampControl : ThingComp
    {
        // todo save tents
        // check for building that can be used as middle "control" thingy
        // turn MapComp into a thingcomp for a thing that's a light and can't be destroyed
        // apply 

        private List<CellRect> campRects;
        private int resourceCount;
        private bool tribal;

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref campRects, "campRects", LookMode.Value);
            Scribe_Values.Look(ref resourceCount, "resourceCount", 0);
            Scribe_Values.Look(ref tribal, "tribal", true);
        }

        public List<CellRect> CampRects { get => campRects; set => campRects = value; }
        public int ResourceCount { get => resourceCount; set => resourceCount = value; }
        public bool Tribal { get => tribal; set => tribal = value; }



    }
}
