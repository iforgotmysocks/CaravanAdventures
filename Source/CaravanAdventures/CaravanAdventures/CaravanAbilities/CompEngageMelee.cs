﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CaravanAdventures.CaravanAbilities
{
    public enum EngageMeleeBehaviour { None, Disabled, Enabled, SavedIndividually };

    internal class CompEngageMelee : ThingComp
    {
        private bool enabled;
        private int ticks = -1;
        private int currentAssignedJobsLoadId;

        public CompProperties_EngageMelee Props => (CompProperties_EngageMelee)props;

        public bool Enabled { get => enabled; set => enabled = value; }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentAssignedJobsLoadId, "currentAssignedJobsLoadId");
            if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.SavedIndividually) Scribe_Values.Look(ref enabled, "engageMeleeEnabled", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.Enabled) enabled = true;
            }
        }

        public override void CompTick()
        {
            if (ticks == -1) ticks = Rand.Range(0, Props.interval);
            if (!enabled) return;

            if (ticks == Props.interval)
            {
                ticks = 0;
                var pawn = parent as Pawn;
                if (InvalidPawnForEngage(pawn, true)) return;
                
                var curTarget = pawn?.CurJob?.targetA.Pawn;
                if (!ModSettings.engageMeleeChaseHostiles
                    && curTarget != null
                    && pawn?.CurJob?.def == JobDefOf.AttackMelee
                    && (curTarget?.CurJobDef == JobDefOf.Flee
                    || curTarget?.CurJobDef == JobDefOf.FleeAndCower
                    || curTarget?.MentalStateDef == MentalStateDefOf.PanicFlee)) pawn.jobs.EndCurrentJob(JobCondition.Succeeded);

                Thing target = (Thing)AttackTargetFinder.BestAttackTarget(pawn,
                    TargetScanFlags.NeedLOSToPawns
                    | TargetScanFlags.NeedLOSToNonPawns
                    | TargetScanFlags.NeedReachableIfCantHitFromMyPos
                    | (ModSettings.engageMeleeChaseHostiles ? TargetScanFlags.NeedThreat : TargetScanFlags.NeedActiveThreat)
                    | TargetScanFlags.NeedAutoTargetable, null, 0f, ModSettings.engageMeleeRange, default(IntVec3), float.MaxValue, false, true, false);

                if (pawn?.CurJob?.playerForced == true)
                {
                    if (pawn?.CurJob?.targetA == target) return;
                    if (pawn?.Position.IsValid == true
                        && pawn?.CurJob?.targetA.IsValid == true
                        && target?.Position.IsValid == true
                        && pawn.Position.DistanceTo(pawn.CurJob.targetA.Cell) <= pawn.Position.DistanceTo(target.Position)) return;
                }
             
                if (target == null) return;
                Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                currentAssignedJobsLoadId = job.loadID;
                pawn.jobs.TryTakeOrderedJob(job);
            }
            ticks++;
        }

        public bool InvalidPawnForEngage(Pawn pawn, bool checkJob = false) => pawn == null
                    || !pawn.Drafted
                    || pawn.Faction != Faction.OfPlayer
                    || (checkJob && (pawn?.CurJob?.playerForced == true && pawn.CurJob.loadID != currentAssignedJobsLoadId))
                    || pawn.WorkTagIsDisabled(WorkTags.Violent)
                    || (pawn?.equipment?.Primary?.def?.IsMeleeWeapon != true && pawn?.equipment?.Primary != null);

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.CompGetGizmosExtra() != null) foreach (var baseGiz in base.CompGetGizmosExtra()) if (baseGiz != null) yield return baseGiz;
            if (InvalidPawnForEngage(parent as Pawn)) yield break;

            yield return new Command_Toggle
            {
                isActive = () => enabled,
                defaultLabel = "CAengageMelee".Translate(),
                defaultDesc = "CAengageMeleeDesc".Translate(),
                Order = 1f,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft", true),
                toggleAction = ToggleAndCheckForActiveJobCancellation,
                hotKey = DefDatabase<KeyBindingDef>.GetNamed("CAEngageMelee")
            };
        }

        private void ToggleAndCheckForActiveJobCancellation()
        {
            enabled = !enabled;

            var pawn = parent as Pawn;
            if (pawn?.CurJob?.loadID != currentAssignedJobsLoadId || enabled) return;
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        }
    }
}
