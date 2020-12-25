﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientProtectiveAura : HediffComp
    {
        private int ticksSortedArray = 0;
        private int ticksSinceStatusCheck = 0;
        private int ticksSincePsyCost = 0;
        private int ticksSincePermHeal = 0;
        private int permTickCount = 10000;
        private Hediff_Injury[] sortedInjuries;
        private bool noInjuries = false;
        private string[] sicknessesToBeHealed = new[] { "WoundInfection", "Flu", "HeartAttack", "FoodPoisoning", "CatatonicBreakdown", "PsychicVertigo", "HeartAttack", "MuscleParasites", "SensoryMechanites", "FibrousMechanites", "GutWorms" };
        private string[] permanentToBeHealed = new[] { "PsychicComa", "Abasia", "Carcinoma", "ChemicalDamageModerate", "ChemicalDamageSevere", "Cirrhosis", "TraumaSavant" };
        private Pawn connector = null;

        public HediffCompProperties_AncientProtectiveAura Props => (HediffCompProperties_AncientProtectiveAura)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref noInjuries, "noInjuries", true);
            //Scribe_Values.Look(ref sortedInjuries, "sortedInjuries", null);
            Scribe_Values.Look(ref ticksSinceStatusCheck, "ticksSinceHeal", 0, false);
            Scribe_Values.Look(ref ticksSincePsyCost, "ticksSincePsyCost", 0, false);
            Scribe_Values.Look(ref ticksSincePermHeal, "ticksSincePermHeal", 0, false);
            Scribe_Values.Look(ref ticksSortedArray, "ticksSortedArray", 0);
            Scribe_References.Look(ref connector, "connector");
        }

        private bool IsGifted(Pawn pawn) => pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientGift) != null;


        public HediffComp_AncientProtectiveAura()
        {
           
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Heal();

            if (ticksSinceStatusCheck > 60)
            {
                if (Props.linked && (Pawn.Faction != Faction.OfPlayer || connector == null || connector.Dead || connector.Faction != Faction.OfPlayer || !IsGifted(connector)))
                {
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
                if (!this.Props.linked && Pawn.psychicEntropy.CurrentPsyfocus == 0f) this.Pawn.health.hediffSet.hediffs.Remove(this.parent);
                ExtinguishFire();
                CureMentalBreaks();
                CureIllnesses();
                ticksSinceStatusCheck = 0;
                if (noInjuries) noInjuries = false;
            }

            if (ticksSincePsyCost > 603)
            {
                if (!this.Props.linked) Pawn.psychicEntropy.OffsetPsyfocusDirectly(-0.002f);
                ticksSincePsyCost = 0;
            }

            if (ticksSincePermHeal > permTickCount)
            {
                if (!ModSettings.onlyHealPermWhenGifted || ModSettings.onlyHealPermWhenGifted && IsGifted(Pawn)) HealPermanent();
                ticksSincePermHeal = 0;
            }

            ticksSortedArray++;
            ticksSinceStatusCheck++;
            ticksSincePsyCost++;
            ticksSincePermHeal++;

            //if (stopwatch.ElapsedMilliseconds > 10) Log.Error(debugString + $" time: {stopwatch.ElapsedMilliseconds} ms");
            //else if (debugString != "") Log.Message(debugString + $" time: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void CureIllnesses()
        {
            var diseases = Pawn.health.hediffSet.hediffs.Where(x => sicknessesToBeHealed.Contains(x.def.defName));
            if (diseases != null && diseases.Count() != 0) Pawn.health.hediffSet.hediffs.RemoveAll(x => diseases.Contains(x));
        }
        
        private void Heal(bool skip = false)
        {
            if (noInjuries) return;
            // added the 60 ticks check as before wounds would constantly heal ignoring the per second interval. when time check on performance again.
            if (sortedInjuries?.Length == null || (ticksSortedArray > 60) && (ticksSortedArray > (sortedInjuries?.Length ?? 0))) ticksSortedArray = 0;
            if (ticksSortedArray == 0) GetAndSortInjuries();
            else
            {
                if (sortedInjuries.Length < ticksSortedArray || sortedInjuries[ticksSortedArray - 1] == null) return;
                var healAmount = ModSettings.healingPerSecond;
                if (sortedInjuries[ticksSortedArray - 1].Severity - healAmount > 0f) sortedInjuries[ticksSortedArray - 1].Severity -= healAmount;
                else Pawn.health.RemoveHediff(sortedInjuries[ticksSortedArray - 1]);

                if (skip) return;
                if (sortedInjuries.Length > 60)
                {
                    ticksSortedArray++;
                    Heal(true);
                }
            }
        }

        private void GetAndSortInjuries()
        {
            this.sortedInjuries = Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().Where(x => !x.IsPermanent()).OrderByDescending(i => i.Severity).ToArray();
            if (sortedInjuries.Count() == 0) noInjuries = true;
        }

        private void CureMentalBreaks()
        {
            if (ModSettings.stopMentalBreaks && Pawn.InAggroMentalState)
            {
                Pawn.MentalState.RecoverFromState();
                // todo add message that aura curred it.
            }
        }

        private void ExtinguishFire()
        {
            if (this.Pawn.HasAttachment(ThingDefOf.Fire))
            {
                var fire = this.Pawn.GetAttachment(ThingDefOf.Fire);
                fire.Destroy();
                // todo add message that aura ext it.
            }
        }

        private void HealPermanent()
        {
            var wound = Pawn.health.hediffSet.hediffs.FirstOrDefault(x => 
                x.IsPermanent() 
            || x.def.chronic 
            || permanentToBeHealed.Contains(x.def.defName));

            if (wound == null) return;
            //Log.Message($"Healing perm wound {wound.def.defName}", true);

            // todo - check this, this doesn't make sense... max severity is 1, this willl always heal... until then we just set it to true to always heal
            var dmgToHeal = wound?.Part == null ? 100 : wound.Part.def.GetMaxHealth(Pawn) / 3f;
            if (wound.Severity - dmgToHeal <= 0f || true)
            {
                if (wound.IsPermanent()) wound.Severity = 0f;
                else HealthUtility.CureHediff(wound);
                if (PawnUtility.ShouldSendNotificationAbout(base.Pawn)) Messages.Message("MessagePermanentWoundHealed".Translate(this.parent.LabelCap, base.Pawn.LabelShort, wound.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent, true);
            }
            else wound.Severity -= dmgToHeal;
        }

       


    }
}
