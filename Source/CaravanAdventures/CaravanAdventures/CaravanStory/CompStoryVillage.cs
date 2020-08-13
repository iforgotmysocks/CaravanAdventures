using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    // probably not gonna need this if i'm gonig to use a Different MP
    class CompStoryVillage : WorldObjectComp
    {
        private int ticks = 0;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            // Add pawn
            // Add mech attack
            Log.Message($"pawn would be generated now");
        }

        public override void CompTick()
        {
            base.CompTick();

            if (ticks >= 2000)
            {

                ticks = 0;
            }
            ticks++;
        }
    }
}
