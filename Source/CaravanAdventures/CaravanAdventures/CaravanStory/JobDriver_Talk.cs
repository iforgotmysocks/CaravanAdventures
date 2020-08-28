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
    class JobDriver_Talk : JobDriver
    {
        private Thing Target => (Thing)base.TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Target, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => this.Target == null);
            var TalkTo = new Toil();
            TalkTo.initAction = delegate ()
            {
                Pawn actor = TalkTo.actor;
                var talkComp = Target.TryGetComp<CompTalk>();
                if (talkComp == null)
                {
                    Log.Message("Didn't find CompTalk on targetThing");
                    this.FailOn(() => true);
                    return;
                }

                var specificAction = talkComp.actions.FirstOrDefault(x => x.Addressed == Target && x.Initiator == pawn && (!x.Finished || x.Repeatable))
                ?? talkComp.actions.FirstOrDefault(x => Target == x.Addressed && x.Initiator == null && (!x.Finished || x.Repeatable));
                if (specificAction == null)
                {
                    Log.Message("Specific Action was null");
                    this.FailOn(() => true);
                    return;
                }
                var instance = FindComponentWithName(specificAction.ClassName);
                var methodInfo = instance.GetType().GetMethod(specificAction.MethodName);
                methodInfo.Invoke(instance, new object[] { pawn, Target });
                specificAction.Finished = true;
            };
            yield return TalkTo;
            yield break;
        }

        private object FindComponentWithName(string className)
        {
            object component = null;
            component = Current.Game.GetComponent(Type.GetType(className));
            if (component == null) component = Find.World.GetComponent(Type.GetType(className));
            if (component == null) component = Find.Maps.Where(x => x.Parent != null && x.Parent.GetComponent(Type.GetType(className)) != null).Select(map => map.Parent).FirstOrDefault();
            if (component == null) component = Find.Maps.Where(x => x.GetComponent(Type.GetType(className)) != null).Select(map => map.GetComponent(Type.GetType(className))).FirstOrDefault();
            return component;
        }
    }
}
