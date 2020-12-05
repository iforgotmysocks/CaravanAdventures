using RimWorld;
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
        private bool bountyNotificationCounterStarted;
        public readonly float bountyNotificationDelay = Helper.Debug() ? 1000 : 60000 * 2;
        private float bountyNotificationCounter;
        private bool bountyServiceAvailable;

        public float BountyPoints { get => bountyPoints; set => bountyPoints = value; }
        public float OngoingAlliedAssistanceDelay { get => ongoingAlliedAssistanceDelay; set => ongoingAlliedAssistanceDelay = value; }
        public float OngoingItemDelay { get => ongoingItemDelay; set => ongoingItemDelay = value; }
        public float OngoingEnvoyDelay { get => ongoingEnvoyDelay; set => ongoingEnvoyDelay = value; }
        public List<Thing> CurrentTradeItemStock { get => currentTradeItemStock; set => currentTradeItemStock = value; }
        public float OngoingVeteranDelay { get => ongoingVeteranDelay; set => ongoingVeteranDelay = value; }
        public float BountyNotificationCounter { get => bountyNotificationCounter; set => bountyNotificationCounter = value; }
        public bool BountyServiceAvailable { get => bountyServiceAvailable; set => bountyServiceAvailable = value; }

        public BountyWC(World world) : base(world)
        {
            bountyPoints = 0f;
            ongoingAlliedAssistanceDelay = -1f;
            ongoingItemDelay = -1f;
            ongoingEnvoyDelay = -1f;
            ongoingVeteranDelay = -1f;
            bountyNotificationCounterStarted = false;
            bountyServiceAvailable = false;
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
            Scribe_Values.Look(ref bountyNotificationCounterStarted, "bountyNotificationCounterStarted", false);
            Scribe_Values.Look(ref bountyNotificationCounter, "bountyNotificationCounter", -1f);
            Scribe_Values.Look(ref bountyServiceAvailable, "bountyServiceAvailable", false);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (CheckCanStartBountyNotificationCounter())
            {
                bountyNotificationCounter = bountyNotificationDelay;
                bountyNotificationCounterStarted = true;
            }

            // todo once settings for replacement faction exists in settings, use that here
            if (bountyNotificationCounter == 0)
            {
                Find.LetterStack.ReceiveLetter("StoryVillage_Response_BountyInitiatedTitle".Translate(), "StoryVillage_Response_BountyInitiatedDesc".Translate(CaravanStory.StoryUtility.FactionOfSacrilegHunters.NameColored, Faction.OfMechanoids.NameColored), LetterDefOf.PositiveEvent);
                bountyServiceAvailable = true;
            }

            bountyNotificationCounter--;
            ongoingAlliedAssistanceDelay--;
            ongoingItemDelay--;
            ongoingEnvoyDelay--;
            ongoingVeteranDelay--;
        }

        private bool CheckCanStartBountyNotificationCounter()
        {
            if (!ModSettings.storyEnabled) return true;
            else return !bountyNotificationCounterStarted && CompCache.StoryWC.storyFlags["IntroVillage_Finished"];
        }

        public string GetNextAvailableDateInDays(float value)
        {
            if (value < 0) return 0.ToString();
            return Math.Round(value / 60000, 1).ToString();
        }
    }
}
