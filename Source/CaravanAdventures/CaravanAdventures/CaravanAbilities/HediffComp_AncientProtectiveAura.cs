using System;
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
        private bool isGifted;
        private Hediff_Injury[] sortedInjuries;
        private bool noInjuries = false;

        public HediffComp_AncientProtectiveAura()
        {
           
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            var gift = Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAura);
            if (gift != null) isGifted = true;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Heal();

            if (ticksSinceStatusCheck > 60)
            {
                if (Pawn.psychicEntropy.CurrentPsyfocus == 0f) this.Pawn.health.hediffSet.hediffs.Remove(this.parent);
                ExtinguishFire();
                CureMentalBreaks();
                CureIllnesses();
                ticksSinceStatusCheck = 0;
                if (noInjuries) noInjuries = false;
            }

            if (ticksSincePsyCost > 603)
            {
                Pawn.psychicEntropy.OffsetPsyfocusDirectly(isGifted ? -0.002f : -0.01f);
                ticksSincePsyCost = 0;
            }
            if (ticksSincePermHeal > 60001)
            {
                if (!ModSettings.Get().onlyHealPermWhenGifted || ModSettings.Get().onlyHealPermWhenGifted && isGifted) HealPermanent();
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
            var diseases = Pawn.health.hediffSet.hediffs.Where(x => new List<string> { "HeartAttack", "Carcinoma", "FoodPoisoning", "CatatonicBreakdown", "PsychicVertigo", "HeartAttack", "MuscleParasites", "SensoryMechanites", "FibrousMechanites", "GutWorms" }.Contains(x.def.defName));
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
                var healAmount = isGifted ? ModSettings.Get().healingPerSecond : (ModSettings.Get().healingPerSecond / 1.5f);
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
            if (ModSettings.Get().stopMentalBreaks && Pawn.InAggroMentalState)
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
            x.IsPermanent() ||
            x.def.chronic ||
            new List<string> { "ChemicalDamageModerate", "ChemicalDamageSevere", "Cirrhosis", "TraumaSavant" }
            .Contains(x.def.defName));

            if (wound == null) return;
            //Log.Message($"Healing perm wound {wound.def.defName}", true);

            var dmgToHeal = wound.Part.def.GetMaxHealth(Pawn) / 3f;
            if (wound.Severity - dmgToHeal <= 0f)
            {
                if (wound.IsPermanent()) wound.Severity = 0f;
                else Pawn.health.RemoveHediff(wound);
                if (PawnUtility.ShouldSendNotificationAbout(base.Pawn)) Messages.Message("MessagePermanentWoundHealed".Translate(this.parent.LabelCap, base.Pawn.LabelShort, wound.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent, true);
            }
            else wound.Severity -= dmgToHeal;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref noInjuries, "noInjuries", true);
            //Scribe_Values.Look(ref sortedInjuries, "sortedInjuries", null);
            Scribe_Values.Look(ref ticksSinceStatusCheck, "ticksSinceHeal", 0, false);
            Scribe_Values.Look(ref ticksSincePsyCost, "ticksSincePsyCost", 0, false);
            Scribe_Values.Look(ref ticksSincePermHeal, "ticksSincePermHeal", 0, false);
            Scribe_Values.Look(ref ticksSortedArray, "ticksSortedArray", 0);
            Scribe_Values.Look(ref isGifted, "isGifted", false, false);
        }


    }
}
