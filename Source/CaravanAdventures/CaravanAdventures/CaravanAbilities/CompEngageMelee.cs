using HarmonyLib;
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
    public enum EngageMeleeBehaviour { None, Disabled, Enabled, SavedIndividually};

    internal class CompEngageMelee : ThingComp
    {
        private bool enabled;
        private int ticks = -1;

        public CompProperties_EngageMelee Props => (CompProperties_EngageMelee)props;

        public bool Enabled { get => enabled; set => enabled = value; }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.SavedIndividually) Scribe_Values.Look(ref enabled, "enabled", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (ModSettings.engageMeleeBehaviour == EngageMeleeBehaviour.Enabled) enabled = true;
            }
        }

        public override void CompTick()
        {
            //base.CompTick();
            if (ticks == -1) ticks = Rand.Range(0, Props.interval);
            if (!enabled) return;

            if (ticks == Props.interval)
            {
                ticks = 0;

                var pawn = parent as Pawn;
                if (!pawn.Drafted 
                    || pawn.Faction != Faction.OfPlayer
                    || pawn?.CurJob?.playerForced == true 
                    || pawn?.CurJob?.def == JobDefOf.AttackMelee
                    || pawn.WorkTagIsDisabled(WorkTags.Violent) 
                    || pawn?.CurrentEffectiveVerb?.IsMeleeAttack != true) return;
                
                Thing target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, 
                    TargetScanFlags.NeedLOSToPawns 
                    | TargetScanFlags.NeedLOSToNonPawns 
                    | TargetScanFlags.NeedReachableIfCantHitFromMyPos 
                    | TargetScanFlags.NeedThreat 
                    | TargetScanFlags.NeedAutoTargetable, null, 0f, ModSettings.engageMeleeRange, default(IntVec3), float.MaxValue, false, true, false);

                //Thing target = null;
                //var cells = GenRadial.RadialCellsAround(pawn.Position, AttackRange, false).Where(cell => cell.Standable(pawn.Map));
                //var pawns = cells.SelectMany(cell => cell.GetThingList(pawn.Map).OfType<Pawn>()).ToList();
                //if (pawns != null && pawns.Count() != 0) target = pawns.FirstOrDefault(x => GenHostility.IsActiveThreatToPlayer(x));

                if (target == null) return;

                Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                //job.maxNumMeleeAttacks = 1;
                //job.expiryInterval = 200;
                //job.reactingToMeleeThreat = true;

                pawn.jobs.TryTakeOrderedJob(job);
            }
            ticks++;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.CompGetGizmosExtra() != null) foreach (var baseGiz in base.CompGetGizmosExtra()) if (baseGiz != null) yield return baseGiz;
            if (!(parent as Pawn).Drafted || (parent as Pawn)?.CurrentEffectiveVerb?.IsMeleeAttack != true) yield break;

            yield return new Command_Toggle
            {
                isActive = () => enabled,
                defaultLabel = "engageMelee".Translate(),
                defaultDesc = "engageMeleeDesc".Translate(),
                Order = 1f,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft", true),
                toggleAction = () =>
                {
                    SoundDefOf.Click.PlayOneShot(null);
                    enabled = !enabled;
                }
            };
        }

     
    }
}
