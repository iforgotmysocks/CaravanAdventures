using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace CaravanAdventures.CaravanStory
{
	public class TalkSet
    {
		public string Id { get; set; }
        public Pawn Initiator { get; set; }
        public Thing Addressed { get; set; }
        public Action<Pawn, Thing> Action { get; set; }
        public bool Finished { get; set; }
		public bool Repeatable { get; set; }
    }

	// todo find out if we can add a exclamation here for questgiver
    public class CompTalk : ThingComp
    {
        public CompProperties_Talk Props => (CompProperties_Talk)props;
		private Pawn approachingPawn = null;
		public bool talkedTo = false;
		public Dictionary<object, Action<Pawn, object>> actions_old = new Dictionary<object, Action<Pawn, object>>();
		public List<TalkSet> actions = new List<TalkSet>();
		public bool Enabled = false;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (!Enabled) yield break;
			if (!actions.Any(x => !x.Finished || x.Repeatable)) yield break;
			// todo - test if detecting by object key works for specific pawns
			if (pawn.Dead || pawn.Drafted)
			{
				yield break;
			}
			string text = "CA_Start_Begin".Translate();
			AcceptanceReport acceptanceReport = this.CanTalkTo(pawn, null);
			if (!acceptanceReport.Accepted && !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
			{
				text = text + ": " + acceptanceReport.Reason;
			}
			
			yield return new FloatMenuOption(text, delegate ()
			{
				ApproachDesiredThing(pawn);
			}, MenuOptionPriority.Default, null, null, 0f, null, null)
			{
				Disabled = !acceptanceReport.Accepted
			};
			yield break;
		}

		private void ApproachDesiredThing(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.CATalk, this.parent);
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

        public AcceptanceReport CanTalkTo(Pawn pawn, LocalTargetInfo? knownSpot = null)
		{
			if (pawn.Dead || pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			if (!pawn.Map.reservationManager.CanReserve(pawn, this.parent, 1, -1, null, false))
			{
				Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(this.parent, pawn);
				return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
			}
			LocalTargetInfo localTargetInfo;
			if (knownSpot != null)
			{
				if (!this.CanUseSpot(pawn, knownSpot.Value))
				{
					return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
		{
			IntVec3 cell = spot.Cell;
			return cell.DistanceTo(this.parent.Position) <= 3.9f && cell.Standable(this.parent.Map) && GenSight.LineOfSight(cell, this.parent.Position, this.parent.Map, false, null, 0, 0) && pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Collections.Look(ref actions, "actions", LookMode.Value);
        }

    }
}
