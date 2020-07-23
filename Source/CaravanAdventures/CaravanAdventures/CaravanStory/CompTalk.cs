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
    public class CompTalk : ThingComp
    {
        public CompProperties_Talk Props => (CompProperties_Talk)props;
		private Pawn approachingPawn = null;
		private bool talkedTo = false;
		private int ticks = 0;
		private int ticksLong = 0;

        public override void CompTick()
        {
			base.CompTick();
			if (ticks >= 25)
			{
				Log.Message($"talkedto: {talkedTo} pawn null? {approachingPawn == null} distance: {(approachingPawn != null ? this.parent.Position.DistanceTo(approachingPawn.Position).ToString() : "pawn null")}");
				if (talkedTo == false && approachingPawn != null && this.parent.Position.DistanceTo(approachingPawn.Position) < 5)
				{
					talkedTo = true;
					Props.actions.FirstOrDefault(x => this.parent == x.Key).Value.Invoke(approachingPawn, this.parent);
				}

				ticks = 0;
			}
			if (ticksLong >= 2000)
			{
				this.parent.TickLong();
				ticksLong = 0;
			}
			ticks++;
			ticksLong++;
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
			Log.Message("Long tick still happening");
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (!Props.enabled) yield break;
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
			Log.Message("Should approach thing now");
			talkedTo = false;
			Job job = JobMaker.MakeJob(JobDefOf.Goto, this.parent);
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			this.approachingPawn = pawn;
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

		public bool TryFindLinkSpot(Pawn pawn, out LocalTargetInfo spot)
		{
			spot = MeditationUtility.FindMeditationSpot(pawn).spot;
			if (this.CanUseSpot(pawn, spot))
			{
				return true;
			}
			int num = GenRadial.NumCellsInRadius(2.9f);
			int num2 = GenRadial.NumCellsInRadius(3.9f);
			for (int i = num; i < num2; i++)
			{
				IntVec3 c = this.parent.Position + GenRadial.RadialPattern[i];
				if (this.CanUseSpot(pawn, c))
				{
					spot = c;
					return true;
				}
			}
			spot = IntVec3.Zero;
			return false;
		}

		private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
		{
			IntVec3 cell = spot.Cell;
			return cell.DistanceTo(this.parent.Position) <= 3.9f && cell.Standable(this.parent.Map) && GenSight.LineOfSight(cell, this.parent.Position, this.parent.Map, false, null, 0, 0) && pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref ticks, "ticks", 0);
			Scribe_Values.Look(ref ticksLong, "ticksLong", 0);
        }

    }
}
