using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using Verse.AI.Group;
using CaravanAdventures.CaravanStory;

namespace CaravanAdventures.CaravanAbilities
{
    class CompAbilityEffect_AncientMechSignal : CompAbilityEffect
    {
        public new CompProperties_AbilityAncientMechSignal Props => (CompProperties_AbilityAncientMechSignal)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = this.parent.pawn.Map;
            var scythers = new List<Pawn>();
            var faction = StoryUtility.CreateOrGetFriendlyMechFaction();

            AffectedCells(target, map).Where(x => x.Standable(map))
                .InRandomOrder().Take(Rand.RangeInclusive(5, 7)).ToList()
                .ForEach(cell => scythers.Add(SpawnScyther(cell, parent.pawn, faction)));

            LordMaker.MakeNewLord(parent.pawn.Faction, new LordJob_AssistColony(parent.pawn.Faction, dest.Cell), map, scythers);
            this.ApplyGoodwillImpact(target, Mathf.RoundToInt(this.parent.def.EffectRadius));
        }

        private Pawn SpawnScyther(IntVec3 intVec, Pawn pawn, Faction faction)
        {
            var scyther = PawnGenerator.GeneratePawn(PawnKindDef.Named("Mech_Scyther"), faction);
            scyther.health.AddHediff(HediffDef.Named("OverheatingBrain"), scyther.health.hediffSet.GetBrain());
            GenSpawn.Spawn(scyther, intVec, pawn.Map, WipeMode.Vanish);
            this.parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(intVec, pawn.Map, 1f), intVec, 60);
            GenClamor.DoClamor(pawn, intVec, 2f, ClamorDefOf.Ability);
            return scyther;
        }

        private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
        {
            if (target.Cell.Filled(this.parent.pawn.Map)) yield break;
            foreach (IntVec3 intVec in GenRadial.RadialCellsAround(target.Cell, this.parent.def.EffectRadius, true))
            {
                // todo line of sight check needed here?
                //if (intVec.InBounds(map) && GenSight.LineOfSightToEdges(target.Cell, intVec, map, true, null))
                if (intVec.InBounds(map)) yield return intVec;
            }
            yield break;
        }

        private void ApplyGoodwillImpact(LocalTargetInfo target, int radius)
        {
            this.affectedFactionCache.Clear();
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(target.Cell, this.parent.pawn.Map, (float)radius, true))
            {
                Pawn pawn;
                if ((pawn = (thing as Pawn)) != null && thing.Faction != null && thing.Faction != this.parent.pawn.Faction && !thing.Faction.HostileTo(this.parent.pawn.Faction) && !this.affectedFactionCache.Contains(thing.Faction) && (base.Props.applyGoodwillImpactToLodgers || !pawn.IsQuestLodger()))
                {
                    this.affectedFactionCache.Add(thing.Faction);
                    thing.Faction.TryAffectGoodwillWith(this.parent.pawn.Faction, base.Props.goodwillImpact, true, true, "GoodwillChangedReason_UsedAbility".Translate(this.parent.def.LabelCap, pawn.LabelShort), new GlobalTargetInfo?(pawn));
                }
            }
            this.affectedFactionCache.Clear();
        }

        private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();
    }
}
