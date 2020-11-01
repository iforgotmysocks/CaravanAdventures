using RimWorld;
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
        //public List<Thing> campAssets;

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref campRects, "campRects", LookMode.Value);
            Scribe_Values.Look(ref resourceCount, "resourceCount", 0);
            Scribe_Values.Look(ref tribal, "tribal", true);
            //Scribe_Collections.Look(ref campAssets, "campAssets", LookMode.Reference);
        }

        public List<CellRect> CampRects { get => campRects; set => campRects = value; }
        public int ResourceCount { get => resourceCount; set => resourceCount = value; }
        public bool Tribal { get => tribal; set => tribal = value; }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var baseOption in base.CompFloatMenuOptions(selPawn)) yield return baseOption;

            yield return new FloatMenuOption("CADeconstructCamp".Translate(), () =>
            {
                // todo destroy everything but only stuff that was spawned and regain resources
                // best create a list of all thingdefs belonging to the camp and kill those
                Messages.Message(new Message($"destroying camp", MessageTypeDefOf.NeutralEvent));
            });
        }


    }
}
