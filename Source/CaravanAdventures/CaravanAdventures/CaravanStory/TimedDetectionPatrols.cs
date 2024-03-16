using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    // Todo add map name to the "come through here" message
    public class TimedDetectionPatrols : WorldObjectComp
    {
        protected const float RaidThreatPointsMultiplier = 2.5f;
        protected int ticksLeftToSendRaid = -1;
        protected int ticksLeftTillNotifyPlayer = -1;
        protected int ticksLeftTillLeaveIfNoEnemies = -1;
        protected bool toggleIncreaseStrenthByCounter = false;
        protected int increaseStrengthCounter = 0;
        protected const int defaultTicksTillLeave = 5000;
        protected List<Lord> lordsToExcludeFromRaidLogic = null;
        protected int raidPoints = 8000;
        protected Faction forcedFaction = null;

        // medieval
        protected List<Hive> hivesToIgnore = null;

        public IncidentDef RaidDef => Helper.ExpRM ? StoryDefOf.CAUnusualInfestation : StoryDefOf.CAMechRaidMixed;
        public string RaidMessage => "MessageCaravanDetectedRaidArrived";

        public bool NextRaidCountdownActiveAndVisible => this.ticksLeftToSendRaid >= 0 && this.ticksLeftTillNotifyPlayer == 0;
        public bool ToggleIncreaseStrenthByCounter { get => toggleIncreaseStrenthByCounter; set => toggleIncreaseStrenthByCounter = value; }
        public string DetectionCountdownTimeLeftString => !this.NextRaidCountdownActiveAndVisible ? "" : TimedDetectionPatrols.GetDetectionCountdownTimeLeftString(this.ticksLeftToSendRaid);
        protected Faction RaidFaction => forcedFaction ?? this.parent.Faction ?? Faction.OfMechanoids;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksLeftToSendRaid, "ticksLeftToForceExitAndRemoveMap", -1, false);
            Scribe_Values.Look<int>(ref this.ticksLeftTillNotifyPlayer, "ticksLeftTillNotifyPlayer", -1, false);
            Scribe_Values.Look(ref ticksLeftTillLeaveIfNoEnemies, "ticksLeftTillLeaveIfNoEnemies", -1, false);
            Scribe_Collections.Look(ref lordsToExcludeFromRaidLogic, "lordsToExcludeFromRaidLogic", LookMode.Reference);
            Scribe_Collections.Look(ref hivesToIgnore, "hivesToIgnore", LookMode.Reference);
            Scribe_Values.Look(ref raidPoints, "raidPoints");
            Scribe_References.Look(ref forcedFaction, "forcedFaction");
            Scribe_Values.Look(ref toggleIncreaseStrenthByCounter, "toggleIncreaseStrenthByCounter", false);
            Scribe_Values.Look(ref increaseStrengthCounter, "increaseStrengthCounter", 0);
        }

        public virtual void Init(Faction forcedFaction = null)
        {
            this.forcedFaction = forcedFaction;
            var mapParent = (MapParent)this.parent;
            if (!mapParent.HasMap) return;
            lordsToExcludeFromRaidLogic = mapParent.Map.lordManager.lords.Where(lord => lord.faction == RaidFaction).ToList();
            hivesToIgnore = Helper.ExpRM ? mapParent.Map.listerThings.ThingsOfDef(ThingDefOf.Hive).Cast<Hive>().Where(hive => hive.Faction == RaidFaction).ToList() : new List<Hive>();
        }

        public void StartDetectionCountdown(int ticks, int notifyTicks = -1, int raidPoints = 8000)
        {
            this.ticksLeftToSendRaid = ticks;
            this.ticksLeftTillNotifyPlayer = ((notifyTicks == -1) ? Mathf.Min((int)(60000f * Rand.Range(1.2f, 1.4f)), ticks / 2) : notifyTicks);
            this.raidPoints = raidPoints;
        }

        public void ResetCountdown() => this.ticksLeftTillNotifyPlayer = (this.ticksLeftToSendRaid = -1);

        public void SetNotifiedSilently() => this.ticksLeftTillNotifyPlayer = 0;

        public override string CompInspectStringExtra()
        {
            string text = null;
            if (this.NextRaidCountdownActiveAndVisible)
            {
                text += "CaravanDetectedRaidCountdown".Translate(this.DetectionCountdownTimeLeftString) + ".\n";
            }
            if (Prefs.DevMode && false)
            {
                if (this.ticksLeftToSendRaid != -1)
                {
                    text = text + "[DEV]: Time left to send raid: " + this.ticksLeftToSendRaid.ToStringTicksToPeriod(true, false, true, true) + "\n";
                }
                if (this.ticksLeftTillNotifyPlayer != -1)
                {
                    text = text + "[DEV]: Time left till notify player about incoming raid: " + this.ticksLeftTillNotifyPlayer.ToStringTicksToPeriod(true, false, true, true) + "\n";
                }
            }
            if (text != null)
            {
                text = text.TrimEndNewlines();
            }
            return text;
        }

        public override void CompTick()
        {
            if (ModSettings.debugMessages) if (ticksLeftToSendRaid % 100 == 0) DLog.Message($"{this.parent.Label}: notify / raid {ticksLeftTillNotifyPlayer} / {ticksLeftToSendRaid}");

            var mapParent = (MapParent)this.parent;
            if (mapParent.HasMap)
            {
                if (this.ticksLeftTillNotifyPlayer > 0)
                {
                    this.ticksLeftTillNotifyPlayer--;
                    if (ticksLeftTillNotifyPlayer == 0)
                    {
                        this.NotifyPlayer();
                    }
                }
                if (this.ticksLeftToSendRaid > 0)
                {
                    this.ticksLeftToSendRaid--;
                    if (this.ticksLeftToSendRaid == 0)
                    {
                        if (Helper.ExpRM) SpawnRaidInsects(mapParent);
                        else SpawnRaid(mapParent);
                        return;
                    }
                    else if (ticksLeftToSendRaid % 7500 == 0) Messages.Message(new Message("Story_Shrine1_NextPatrolWarning".Translate(this.parent.Label, GetDetectionCountdownTimeLeftString(ticksLeftToSendRaid)), ModSettings.mutedShrineMessages ? MessageTypeDefOf.SilentInput : MessageTypeDefOf.ThreatBig), false);
                }
                if (ticksLeftTillLeaveIfNoEnemies > 0)
                {
                    ticksLeftTillLeaveIfNoEnemies--;
                    if (ticksLeftTillLeaveIfNoEnemies == defaultTicksTillLeave - 2000) MakeInsectsAngry(mapParent);
                    if (ticksLeftTillLeaveIfNoEnemies == 0)
                    {
                        if (Helper.ExpRM) HaveRaidRetreatInsects(mapParent);
                        else HaveRaidRetreat(mapParent);
                    }
                }
            }
            else
            {
                this.ResetCountdown();
            }
        }

        protected virtual void SpawnRaid(MapParent mapParent)
        {
            lordsToExcludeFromRaidLogic = mapParent.Map.lordManager.lords.Where(lord => lord.faction == RaidFaction).ToList();
            var incidentParms = new IncidentParms
            {
                target = mapParent.Map,
                //incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
                points = raidPoints * (1 + increaseStrengthCounter * 0.2f),
                faction = this.RaidFaction,
                raidArrivalMode = Helper.ExpRM ? null : PawnsArrivalModeDefOf.EdgeWalkIn,
                raidStrategy = Helper.ExpRM ? null : RaidStrategyDefOf.ImmediateAttack,
                customLetterDef = ModSettings.mutedShrineMessages ? LetterDefOf.NeutralEvent : LetterDefOf.ThreatBig
            };
            DLog.Message($"Default threat points: {StorytellerUtility.DefaultThreatPointsNow(incidentParms.target)}");
            if (!Helper.RunSafely(() => RaidDef.Worker.TryExecute(incidentParms)))
            {
                DLog.Message($"Creating raid failed, trying again in 1 hour");
                this.ticksLeftToSendRaid = 2500;
                return;
            }
            this.ticksLeftToSendRaid = (int)(Rand.Range(18f, 24f) * 2500f);
            ticksLeftTillLeaveIfNoEnemies = defaultTicksTillLeave;
            if (toggleIncreaseStrenthByCounter) increaseStrengthCounter++;
            Messages.Message(RaidMessage.Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction, this.ticksLeftToSendRaid.ToStringTicksToDays("F1")), ModSettings.mutedShrineMessages ? MessageTypeDefOf.SilentInput : MessageTypeDefOf.ThreatBig, true);
        }

        private void MakeInsectsAngry(MapParent mapParent)
        {
            if (!Helper.ExpRM) return;
            DLog.Message($"patrol started making insects angry");
            var insects = mapParent.Map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.Faction == RaidFaction);
            foreach (var insect in insects) insect.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        }

        protected virtual void HaveRaidRetreat(MapParent mapParent)
        {
            var raidLords = mapParent.Map.lordManager.lords.Where(lord => lord.faction == RaidFaction && !lordsToExcludeFromRaidLogic.Contains(lord));
            DLog.Message($"raid mechs: {raidLords.Select(lord => lord.ownedPawns).Count()}");
            if (!raidLords.Any(lord => lord.AnyActivePawn)) ticksLeftTillLeaveIfNoEnemies = -1;
            else if (GenHostility.AnyHostileActiveThreatTo(mapParent.Map, RaidFaction)) ticksLeftTillLeaveIfNoEnemies = 3000;
            else
            {
                foreach (var lord in raidLords.Reverse())
                {
                    var pawnsToReassign = lord.ownedPawns;
                    lord.lordManager.RemoveLord(lord);
                    LordMaker.MakeNewLord(RaidFaction, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Jog, true, true), pawnsToReassign.First().Map, pawnsToReassign);
                }
                ticksLeftTillLeaveIfNoEnemies = -1;
            }
        }

        protected void SpawnRaidInsects(MapParent mapParent)
        {
            var insectsToIgnore = mapParent.Map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.Faction == RaidFaction).ToList();
            hivesToIgnore = mapParent.Map.listerThings.ThingsOfDef(ThingDefOf.Hive).Cast<Hive>().Where(hive => hive.Faction == RaidFaction).ToList();
            SpawnRaid(mapParent);
        }

        protected void HaveRaidRetreatInsects(MapParent mapParent)
        {
            var raidLords = mapParent.Map.lordManager.lords.Where(lord => lord.faction == RaidFaction && !lordsToExcludeFromRaidLogic.Contains(lord));
            DLog.Message($"raid mechs: {raidLords.Select(lord => lord.ownedPawns).Count()}");
            if (!raidLords.Any(lord => lord.AnyActivePawn)) ticksLeftTillLeaveIfNoEnemies = -1;
            else if (GenHostility.AnyHostileActiveThreatTo(mapParent.Map, RaidFaction)) ticksLeftTillLeaveIfNoEnemies = 3000;
            else
            {
                DLog.Message($"patrol started leaving");
                var hivesExceptHivesToIgnore = mapParent.Map.listerThings.ThingsOfDef(ThingDefOf.Hive).Cast<Hive>().Where(hive => hive.Faction == RaidFaction && !hivesToIgnore.Contains(hive));
                foreach (var hive in hivesExceptHivesToIgnore.Reverse()) hive.Destroy();
                foreach (var lord in raidLords.Reverse())
                {
                    var pawnsToReassign = lord.ownedPawns;
                    lord.lordManager.RemoveLord(lord);
                    foreach (var pawn in pawnsToReassign) pawn.MentalState.RecoverFromState();
                    LordMaker.MakeNewLord(RaidFaction, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Jog, true, true), pawnsToReassign.First().Map, pawnsToReassign);
                }
                ticksLeftTillLeaveIfNoEnemies = -1;
            }
        }

        private void NotifyPlayer()
        {
            Find.LetterStack.ReceiveLetter("LetterLabelSiteCountdownStarted".Translate(), "Story_Shrine1_ArrivalTimeShownLetterMessage".Translate(this.ticksLeftToSendRaid.ToStringTicksToDays("F1"), this.RaidFaction.def.pawnsPlural, this.RaidFaction), LetterDefOf.ThreatBig, this.parent, null, null, null, null);
        }

        public static string GetDetectionCountdownTimeLeftString(int ticksLeft)
        {
            if (ticksLeft < 0) return "";
            return ticksLeft.ToStringTicksToPeriod(true, false, true, true);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Set raid timer to 1 hour",
                    action = () => this.ticksLeftToSendRaid = 2500
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Set notify raid timer to 1 hour",
                    action = () => this.ticksLeftTillNotifyPlayer = 2500
                };
            }
            yield break;
        }

        public void CopyFrom(TimedDetectionPatrols other)
        {
            this.ticksLeftToSendRaid = other.ticksLeftToSendRaid;
            this.ticksLeftTillNotifyPlayer = other.ticksLeftTillNotifyPlayer;
        }


    }
}
