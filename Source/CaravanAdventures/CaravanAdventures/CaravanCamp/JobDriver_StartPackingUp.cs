using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CaravanAdventures.CaravanCamp
{
    class JobDriver_StartPackingUP : JobDriver
    {
        protected ThingWithComps ControlSpot => this.job.GetTarget(TargetIndex.A).Thing as ThingWithComps;
        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);

        private CompCampControl comp;
        private int durationForOneTent = 60;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            comp = ControlSpot.TryGetComp<CompCampControl>();
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => this.pawn.Drafted);
            var waitToil = Toils_General.Wait(comp.CampRects.Count * durationForOneTent, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).WithEffect(EffecterDefOf.Deflect_Metal_Bullet, TargetIndex.A);
            var skip = false;
            waitToil.tickAction = () =>
            {
                if (Find.TickManager.TicksGame % (durationForOneTent / 2) == 0 && comp.CampRects.Count > 1)
                {
                    if (!skip) CampDefOf.CAPackUpTent.PlayOneShot(new TargetInfo(ControlSpot.Position, ControlSpot.Map, false));
                    skip = !skip;
                }

                if (Find.TickManager.TicksGame % durationForOneTent == 0)
                {
                    comp.PackUpTentAtRandomRect();
                }
            };
            yield return waitToil;
            yield return new Toil()
            {
                initAction = () =>
                {
                    comp.FinishPackingReainingTentsAndControl();
                }
            };
        }
    }
}
