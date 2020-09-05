using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
	// Todo add map name to the "come through here" message
    public class TimedDetectionPatrols : WorldObjectComp
	{
		public const float RaidThreatPointsMultiplier = 2.5f;
		private int ticksLeftToSendRaid = -1;
		private int ticksLeftTillNotifyPlayer = -1;
		private int ticksLeftTillLeaveIfNoEnemies = -1;
        private const int defaultTicksTillLeave = 5000;
		private List<Lord> lordsToExcludeFromRaidLogic = null;
		private int raidPoints = 8000;

		public bool NextRaidCountdownActiveAndVisible
		{
			get
			{
				return this.ticksLeftToSendRaid >= 0 && this.ticksLeftTillNotifyPlayer == 0;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeftToSendRaid, "ticksLeftToForceExitAndRemoveMap", -1, false);
			Scribe_Values.Look<int>(ref this.ticksLeftTillNotifyPlayer, "ticksLeftTillNotifyPlayer", -1, false);
			Scribe_Values.Look(ref ticksLeftTillLeaveIfNoEnemies, "ticksLeftTillLeaveIfNoEnemies", -1, false);
			Scribe_Collections.Look(ref lordsToExcludeFromRaidLogic, "lordsToExcludeFromRaidLogic", LookMode.Reference);
			Scribe_Values.Look(ref raidPoints, "raidPoints");
		}

		public void Init()
        {
			var mapParent = (MapParent)this.parent;
			if (!mapParent.HasMap) return;
			lordsToExcludeFromRaidLogic = mapParent.Map.lordManager.lords.Where(lord => lord.faction == Faction.OfMechanoids).ToList();
		}

		public string DetectionCountdownTimeLeftString
		{
			get
			{
				if (!this.NextRaidCountdownActiveAndVisible)
				{
					return "";
				}
				return TimedDetectionPatrols.GetDetectionCountdownTimeLeftString(this.ticksLeftToSendRaid);
			}
		}

		private Faction RaidFaction
		{
			get
			{
				return this.parent.Faction ?? Faction.OfMechanoids;
			}
		}

		public void StartDetectionCountdown(int ticks, int notifyTicks = -1, int raidPoints = 8000)
		{
			this.ticksLeftToSendRaid = ticks;
			this.ticksLeftTillNotifyPlayer = ((notifyTicks == -1) ? Mathf.Min((int)(60000f * Rand.Range(1.2f, 1.4f)), ticks / 2) : notifyTicks);
			this.raidPoints = raidPoints;
		}

		public void ResetCountdown()
		{
			this.ticksLeftTillNotifyPlayer = (this.ticksLeftToSendRaid = -1);
		}

		public void SetNotifiedSilently()
		{
			this.ticksLeftTillNotifyPlayer = 0;
		}

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
			if(ModSettings.Get().debug) if (ticksLeftToSendRaid % 100 == 0) Log.Message($"{this.parent.Label}: notify / raid {ticksLeftTillNotifyPlayer} / {ticksLeftToSendRaid}");

			// todo changed from AncientShrineMP to generall MP, check if shrine still works!
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
                        var incidentParms = new IncidentParms
                        {
                            target = mapParent.Map,
                            //incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
                            points = raidPoints,
                            faction = this.RaidFaction,
                            raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
                        };
                        Log.Message($"Default threat points: {StorytellerUtility.DefaultThreatPointsNow(incidentParms.target)}");
						IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
						this.ticksLeftToSendRaid = (int)(Rand.Range(18f, 24f) * 2500f);
						ticksLeftTillLeaveIfNoEnemies = defaultTicksTillLeave;
						Messages.Message("MessageCaravanDetectedRaidArrived".Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction, this.ticksLeftToSendRaid.ToStringTicksToDays("F1")), MessageTypeDefOf.ThreatBig, true);
						return;
					}
					else if (ticksLeftToSendRaid % 7500 == 0) Messages.Message(new Message("Story_Shrine1_NextPatrolWarning".Translate(this.parent.Label, GetDetectionCountdownTimeLeftString(ticksLeftToSendRaid)), MessageTypeDefOf.ThreatBig), false);
				}
				if (ticksLeftTillLeaveIfNoEnemies > 0)
				{
					ticksLeftTillLeaveIfNoEnemies--;
					if (ticksLeftTillLeaveIfNoEnemies == 0)
					{
						var raidLords = mapParent.Map.lordManager.lords.Where(lord => lord.faction == Faction.OfMechanoids && !lordsToExcludeFromRaidLogic.Contains(lord));
						Log.Message($"raid mechs: {raidLords.Select(lord => lord.ownedPawns).Count()}");
						if (!raidLords.Any(lord => lord.AnyActivePawn)) ticksLeftTillLeaveIfNoEnemies = -1;
						else if (GenHostility.AnyHostileActiveThreatTo(mapParent.Map, Faction.OfMechanoids)) ticksLeftTillLeaveIfNoEnemies = 1000;
						else
						{
							foreach (var lord in raidLords.Reverse())
							{
								var pawnsToReassign = lord.ownedPawns;
								lord.lordManager.RemoveLord(lord);
								LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Jog, true, true), pawnsToReassign.First().Map, pawnsToReassign);
							}
							ticksLeftTillLeaveIfNoEnemies = -1;
						}
						
					}
				}
			}
			else
			{
				this.ResetCountdown();
			}
		}

		private void NotifyPlayer()
		{
			Find.LetterStack.ReceiveLetter("LetterLabelSiteCountdownStarted".Translate(), "Story_Shrine1_ArrivalTimeShownLetterMessage".Translate(this.ticksLeftToSendRaid.ToStringTicksToDays("F1"), this.RaidFaction.def.pawnsPlural, this.RaidFaction), LetterDefOf.ThreatBig, this.parent, null, null, null, null);
		}

		public static string GetDetectionCountdownTimeLeftString(int ticksLeft)
		{
			if (ticksLeft < 0)
			{
				return "";
			}
			return ticksLeft.ToStringTicksToPeriod(true, false, true, true);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Set raid timer to 1 hour",
					action = delegate ()
					{
						this.ticksLeftToSendRaid = 2500;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Set notify raid timer to 1 hour",
					action = delegate ()
					{
						this.ticksLeftTillNotifyPlayer = 2500;
					}
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
