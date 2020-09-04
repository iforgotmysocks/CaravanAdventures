using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CaravanAdventures.CaravanStory
{
	public class TalkSet : IExposable
	{
		private string id;
		private Pawn initiator;
		private Thing addressed;
		private bool finished;
		private bool repeatable;
		private string className;
		private string methodName;

		public string Id { get => id; set => id = value; }
		public Pawn Initiator { get => initiator; set => initiator = value; }
		public Thing Addressed { get => addressed; set => addressed = value; }
		public bool Finished { get => finished; set => finished = value; }
		public bool Repeatable { get => repeatable; set => repeatable = value; }
        public string ClassName { get => className; set => className = value; }
        public string MethodName { get => methodName; set => methodName = value; }

        public void ExposeData()
		{
			Scribe_Values.Look(ref id, "id");
			Scribe_References.Look(ref initiator, "initiator");
			Scribe_References.Look(ref addressed, "addressed");
			Scribe_Values.Look(ref finished, "finished");
			Scribe_Values.Look(ref repeatable, "repeatable");
			Scribe_Values.Look(ref className, "className");
			Scribe_Values.Look(ref methodName, "methodName");
		}
	}

	public class CompTalk : ThingComp
	{
		public List<TalkSet> actionsCt = new List<TalkSet>();
		private bool enabledCt = false;
		private bool showQuestionMarkCt;
        public bool Enabled { get => (parent is Pawn pawn && !pawn.Dead || !(parent is Pawn)) && enabledCt; set => enabledCt = value; }
        public bool ShowQuestionMark { 
			get => Enabled && (IgnoreQuestionMarkLogic ? showQuestionMarkCt : HasUnfinishedDialogs());
			set => showQuestionMarkCt = value; 
		}
        public bool IgnoreQuestionMarkLogic { get; set; }
        public CompProperties_Talk Props => (CompProperties_Talk)props;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref actionsCt, "actionsCt", LookMode.Deep);
			Scribe_Values.Look(ref enabledCt, "enabledCt");
			Scribe_Values.Look(ref showQuestionMarkCt, "showQuestionMarkCt");

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.parent != null && actionsCt == null) actionsCt = new List<TalkSet>();
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (this.parent == null)
			{
				Log.Error($"CompTalk parent is null...");
				yield break;
			}
			if (!Enabled || actionsCt == null || actionsCt.Count == 0) yield break;
			if (!actionsCt.Any(x => !x.Finished || x.Repeatable)) yield break;
			if (pawn.Dead || pawn.Drafted) yield break;
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
				Disabled = !acceptanceReport.Accepted,
				revalidateClickTarget = this.parent,
			};
			yield break;
		}

		private void ApproachDesiredThing(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.CATalk, this.parent);
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

		public bool TalkedTo() => actionsCt.Any(action => action.Finished);
		public bool HasUnfinishedDialogs() => actionsCt.Any(action => !action.Finished && !action.Repeatable);

		public AcceptanceReport CanTalkTo(Pawn pawn, LocalTargetInfo? knownSpot = null)
		{
			if (pawn.Dead || pawn.Faction != Faction.OfPlayer) return false;
            if (!pawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn)) return new AcceptanceReport("can't reach");
            if (!pawn.Map.reservationManager.CanReserve(pawn, this.parent, 1, -1, null, false))
            {
                Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(this.parent, pawn);
                return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
            }
            //if (knownSpot != null)
            //{
            //	if (!this.CanUseSpot(pawn, knownSpot.Value))
            //	{
            //		return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
            //	}
            //}
            return AcceptanceReport.WasAccepted;
		}

		private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
		{
			IntVec3 cell = spot.Cell;
			return cell.DistanceTo(this.parent.Position) <= 3.9f && cell.Standable(this.parent.Map) && GenSight.LineOfSight(cell, this.parent.Position, this.parent.Map, false, null, 0, 0) && pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

	}
}
