using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace CaravanAdventures.CaravanAbilities
{
    class CompAbilityEffect_AncientThunderBold : CompAbilityEffect
    {
        public new CompProperties_AbilityAncientThunderBold Props => (CompProperties_AbilityAncientThunderBold)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = this.parent.pawn.Map;

            foreach (IntVec3 intVec in this.AffectedCells(target, map))
            {
                this.ApplyDamage(intVec.GetThingList(map), this.parent.pawn);
                if (Rand.Bool) map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, intVec));
            }
            
            this.ApplyGoodwillImpact(target, Mathf.RoundToInt(this.parent.def.EffectRadius));
        }

        private void ApplyDamage(List<Thing> things, Pawn pawnToExclude)
        {
            var relevantThings = things.Where(x => x is Pawn || x is Building).ToList();
            var waist = DefDatabase<BodyPartDef>.GetNamed("Waist");

            for (var i = relevantThings.Count - 1; i >= 0; i--)
            {
                var thing = relevantThings[i];
                if (thing is Pawn)
                {
                    var pawn = (Pawn)thing;
                    if (pawn == pawnToExclude || pawn.Dead) continue;
                    var isBoss = CompCache.StoryWC.BossDefs().Contains(pawn.def);
                    var count = Rand.Range(5, 8);
                    foreach (var part in Helper.PickSomeInRandomOrder(pawn.RaceProps.body.AllParts.Where(part => part.def != AbilityDefOf.Finger && part.def != AbilityDefOf.Toe), count))
                    {
                        if (pawn == null | pawn.Dead) break;
                        if (part.def == waist) continue;

                        var damage = part.def.GetMaxHealth(pawn) * (isBoss ? 0.2f : Rand.Range(0.5f, 0.75f));
                        if (Rand.Chance(pawn.RaceProps.IsMechanoid ? ModSettings.mechanoidDissmemberChance : ModSettings.humanDissmemberChance))
                        {
                            damage *= isBoss ? 3 : 5;
                        }

                        DLog.Message($"Calculated {damage} damage for {pawn.Name} with bodypart {part.Label}.");
                        pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, damage, 2f, -1, parent.pawn, part));
                    }
                }
                else if (thing is Building building)
                {
                    if (!building.Destroyed) building.TakeDamage(new DamageInfo(DamageDefOf.Burn, building.MaxHitPoints * Rand.Range(ModSettings.additionalBuildingAreaDamageMin, ModSettings.additionalBuildingAreaDamageMax), 200));
                }
            }
        }

        private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
        {
            if (target.Cell.Filled(this.parent.pawn.Map)) yield break;
            foreach (IntVec3 intVec in GenRadial.RadialCellsAround(target.Cell, this.parent.def.EffectRadius, true))
            {
                if (intVec.InBounds(map)) yield return intVec;
            }
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
                    thing.Faction.TryAffectGoodwillWith(this.parent.pawn.Faction, base.Props.goodwillImpact, true, true, HistoryEventDefOf.UsedHarmfulAbility, new GlobalTargetInfo?(pawn));
                }
            }
            this.affectedFactionCache.Clear();
        }

        private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();
    }
}
