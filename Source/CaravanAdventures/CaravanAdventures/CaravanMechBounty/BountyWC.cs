using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyWC : WorldComponent
    {
        private float bountyPoints;
        private float ongoingAlliedAssistanceDelay;

        public float BountyPoints { get => bountyPoints; set => bountyPoints = value; }
        public float OngoingAlliedAssistanceDelay { get => ongoingAlliedAssistanceDelay; set => ongoingAlliedAssistanceDelay = value; }

        public BountyWC(World world) : base(world)
        {
            bountyPoints = 0f;
            ongoingAlliedAssistanceDelay = -1f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref bountyPoints, "bountyPoints", 0f);
            Scribe_Values.Look(ref ongoingAlliedAssistanceDelay, "ongoingAlliedAssistanceDelay", -1f);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            ongoingAlliedAssistanceDelay--;
        }
    }
}
