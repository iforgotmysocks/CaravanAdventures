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
        private float ongoingItemDelay;
        private float ongoingEnvoyDelay;
        private float ongoingVeteranDelay;
        private List<Thing> currentTradeItemStock;

        public float BountyPoints { get => bountyPoints; set => bountyPoints = value; }
        public float OngoingAlliedAssistanceDelay { get => ongoingAlliedAssistanceDelay; set => ongoingAlliedAssistanceDelay = value; }
        public float OngoingItemDelay { get => ongoingItemDelay; set => ongoingItemDelay = value; }
        public float OngoingEnvoyDelay { get => ongoingEnvoyDelay; set => ongoingEnvoyDelay = value; }
        public List<Thing> CurrentTradeItemStock { get => currentTradeItemStock; set => currentTradeItemStock = value; }
        public float OngoingVeteranDelay { get => ongoingVeteranDelay; set => ongoingVeteranDelay = value; }

        public BountyWC(World world) : base(world)
        {
            bountyPoints = 0f;
            ongoingAlliedAssistanceDelay = -1f;
            ongoingItemDelay = -1f;
            ongoingEnvoyDelay = -1f;
            ongoingVeteranDelay = -1f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref bountyPoints, "bountyPoints", 0f);
            Scribe_Values.Look(ref ongoingAlliedAssistanceDelay, "ongoingAlliedAssistanceDelay", -1f);
            Scribe_Values.Look(ref ongoingItemDelay, "ongoingItemDelay", -1f);
            Scribe_Values.Look(ref ongoingEnvoyDelay, "ongoingEnvoyDelay", -1f);
            Scribe_Values.Look(ref ongoingVeteranDelay, "ongoingVeteranDelay", -1f);
            Scribe_Collections.Look(ref currentTradeItemStock, "currentTradeItemStock", LookMode.Deep);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            ongoingAlliedAssistanceDelay--;
            ongoingItemDelay--;
            ongoingEnvoyDelay--;
            ongoingVeteranDelay--;
        }

        public string GetNextAvailableDateInDays(float value)
        {
            if (value < 0) return 0.ToString();
            return Math.Round(value / 60000, 1).ToString();
        }
    }
}
