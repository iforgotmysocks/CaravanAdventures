using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    public class TimedDetectionPatrols : WorldObjectComp
	{
		public const float RaidThreatPointsMultiplier = 2.5f;
		private int ticksLeftToSendRaid = -1;
		private int ticksLeftTillNotifyPlayer = -1;
		private int ticksLeftTillLeaveIfNoEnemies = -1;
		private const int defaultTicksTillLeave = 7500;
		private static List<Pawn> tmpPawns = new List<Pawn>();

		public bool NextRaidCountdownActiveAndVisible
		{
			get
			{
				return this.ticksLeftToSendRaid >= 0 && this.ticksLeftTillNotifyPlayer == 0;
			}
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

		public void StartDetectionCountdown(int ticks, int notifyTicks = -1)
		{
			this.ticksLeftToSendRaid = ticks;
			this.ticksLeftTillNotifyPlayer = ((notifyTicks == -1) ? Mathf.Min((int)(60000f * Rand.Range(1.2f, 1.4f)), ticks / 2) : notifyTicks);
		}

		public void ResetCountdown()
		{
			this.ticksLeftTillNotifyPlayer = (this.ticksLeftToSendRaid = -1);
		}

		public void SetNotifiedSilently()
		{
			this.ticksLeftTillNotifyPlayer = 0;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeftToSendRaid, "ticksLeftToForceExitAndRemoveMap", -1, false);
			Scribe_Values.Look<int>(ref this.ticksLeftTillNotifyPlayer, "ticksLeftTillNotifyPlayer", -1, false);
			Scribe_Values.Look(ref ticksLeftTillLeaveIfNoEnemies, "ticksLeftTillLeaveIfNoEnemies", -1, false);
		}

		public override string CompInspectStringExtra()
		{
			Log.Message($"string being build");
			string text = null;
			if (this.NextRaidCountdownActiveAndVisible)
			{
				text += "CaravanDetectedRaidCountdown".Translate(this.DetectionCountdownTimeLeftString) + ".\n";
			}
			if (Prefs.DevMode)
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
			Log.Message($"returning: {text}");
			return text;
		}

		public override void CompTick()
		{
			// todo add timer to check for mechs not part of the generated list (and not asleep) and send them off map to die, when they don't have anything to fight

			var mapParent = (AncientMasterShrineMP)this.parent;
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
						IncidentParms incidentParms = new IncidentParms();
						incidentParms.target = mapParent.Map;
						//incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target) * 2.5f;
						incidentParms.points = 8000;
						incidentParms.faction = this.RaidFaction;
						incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
						IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
						this.ticksLeftToSendRaid = (int)(Rand.Range(18f, 24f) * 2500f);
						ticksLeftTillLeaveIfNoEnemies = defaultTicksTillLeave; 
						Log.Message($"ticksTillLeave: {ticksLeftTillLeaveIfNoEnemies}");
						Messages.Message("MessageCaravanDetectedRaidArrived".Translate(incidentParms.faction.def.pawnsPlural, incidentParms.faction, this.ticksLeftToSendRaid.ToStringTicksToDays("F1")), MessageTypeDefOf.ThreatBig, true);
						return;
					}
					else if (ticksLeftToSendRaid % 7500 == 0) Messages.Message(new Message("Story_Shrine1_NextPatrolWarning".Translate(GetDetectionCountdownTimeLeftString(ticksLeftToSendRaid)), MessageTypeDefOf.ThreatBig), false);
				}
				if (ticksLeftTillLeaveIfNoEnemies > 0)
                {
					if (ticksLeftTillLeaveIfNoEnemies % 100 == 0) Log.Message($"Check if mechs will leave {ticksLeftTillLeaveIfNoEnemies}");
					ticksLeftTillLeaveIfNoEnemies--;
					if (ticksLeftTillLeaveIfNoEnemies == 0)
                    {
						Log.Message("Check happening");
						var raidMechs = mapParent.Map.mapPawns.PawnsInFaction(Faction.OfMechanoids).Where(mech => !mapParent.generatedMechs.Contains(mech) && mech.Awake());
						Log.Message($"mechcount: {raidMechs.Count()}");
						if (raidMechs.Count() == 0) ticksLeftTillLeaveIfNoEnemies = -1;
						else if (GenHostility.AnyHostileActiveThreatTo(mapParent.Map, Faction.OfMechanoids)) ticksLeftTillLeaveIfNoEnemies = 1000;
						else
                        {
							var lord = raidMechs.FirstOrDefault(mech => mech.GetLord() != null)?.GetLord();
							Log.Message($"type: {lord.GetType()}");
							lord.SetJob(new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.None, true, true));
							ticksLeftTillLeaveIfNoEnemies = -1;
							
							//LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Jog, true, true), mapParent.Map, raidMechs);
							//raidMechs.ToList().ForEach(mech => mech.GetLord()?.SetJob(new LordJob_ExitMapBest(Verse.AI.LocomotionUrgency.Jog, true, true)));
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
			Find.LetterStack.ReceiveLetter("LetterLabelSiteCountdownStarted".Translate(), "LetterTextSiteCountdownStarted".Translate(this.ticksLeftToSendRaid.ToStringTicksToDays("F1"), this.RaidFaction.def.pawnsPlural, this.RaidFaction), LetterDefOf.ThreatBig, this.parent, null, null, null, null);
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
