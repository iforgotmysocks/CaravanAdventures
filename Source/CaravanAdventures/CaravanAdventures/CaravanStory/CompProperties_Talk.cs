using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    public class CompProperties_Talk : CompProperties
    {
        public CompProperties_Talk()
        {
            this.compClass = typeof(CompTalk);
        }

        public bool enabled = false;
        public Dictionary<object, Action<Pawn, object>> actions = new Dictionary<object, Action<Pawn, object>>();
    }
}
