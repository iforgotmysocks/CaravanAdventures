using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientProtectiveAura : HediffComp
    {
        private int ticksSortedArray = 0;
        private int ticksSinceStatusCheck = 0;
        private int ticksSincePsyCost = 0;
        private int ticksSincePermHeal = 0;
        private int ticksSinceHeatCheck = 0;
        private int permTickCount = new IntRange(10000, 12000).RandomInRange;
        private Hediff_Injury[] sortedInjuries;
        private bool noInjuries = false;
        private string[] sicknessesToBeHealed = new[] { "WoundInfection", "Flu", "Plague", "HeartAttack", "FoodPoisoning", "CatatonicBreakdown", "PsychicVertigo", "HeartAttack", "MuscleParasites", "SensoryMechanites", "FibrousMechanites", "GutWorms" };
        private string[] permanentToBeHealed = new[] { "PsychicComa", "Abasia", "Carcinoma", "ChemicalDamageModerate", "ChemicalDamageSevere", "Cirrhosis", "TraumaSavant" };
        private Pawn connector = null;
        private int statusCheckTickCount = new IntRange(55, 65).RandomInRange;
        private int heatCheckTickCount = new IntRange(300, 400).RandomInRange;
        private bool protectsTheShip = false;

        public bool ProtectsTheShip => protectsTheShip;

        public HediffCompProperties_AncientProtectiveAura Props => (HediffCompProperties_AncientProtectiveAura)props;

        public Pawn Connector { get => connector; set => connector = value; }

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
            Scribe_Values.Look(ref protectsTheShip, "protectsTheShip", false);
        }

        private bool IsGifted(Pawn pawn) => pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientGift);
        private bool IsCoordinatorActive(Pawn pawn) => pawn.health.hediffSet.HasHediff(AbilityDefOf.CAAncientCoordinator);

        public HediffComp_AncientProtectiveAura()
        {

        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (!Props.linked)
            {
                var linkedHediff = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == AbilityDefOf.CAAncientProtectiveAuraLinked);
                if (linkedHediff != null) Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Heal();

            if (ticksSinceStatusCheck > statusCheckTickCount)
            {
                ticksSinceStatusCheck = 0;
                if (Props.linked && (Pawn.Faction != Faction.OfPlayer || connector == null || connector.Dead || connector.Faction != Faction.OfPlayer || !IsGifted(connector) || !IsCoordinatorActive(connector)))
                {
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
                if (!this.Props.linked && Pawn.psychicEntropy.CurrentPsyfocus == 0f) this.Pawn.health.hediffSet.hediffs.Remove(this.parent);
                ExtinguishFire();
                CureMentalBreaks();
                CureIllnesses();
                if (noInjuries) noInjuries = false;
            }

            if (ticksSinceHeatCheck > heatCheckTickCount)
            {
                ticksSinceHeatCheck = 0;
                if (ModSettings.regulateBodyTemperature) ReduceHeatOrCold();
            }

            if ((ticksSincePsyCost == 100 || ticksSincePsyCost == 400) && Pawn?.Map != null && ModSettings.enableAncientAuraAnimation) Helper.RunSavely(() =>
            {
                if (!ModsConfig.RoyaltyActive) return;
                FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(Pawn, DefDatabase<FleckDef>.GetNamedSilentFail("AncientProtectiveAuraFleck"), Vector3.zero, 0.125f);
                if (!dataAttachedOverlay.Equals(default(FleckCreationData)))
                {
                    dataAttachedOverlay.link = new FleckAttachLink(Pawn);
                    Pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
                }
            });

            if (ticksSincePsyCost > 603)
            {
                ticksSincePsyCost = 0;
                if (!this.Props.linked) Pawn.psychicEntropy.OffsetPsyfocusDirectly(IsGifted(Pawn) ? -0.002f : -0.02f);
            }

            if (ticksSincePermHeal > permTickCount)
            {
                ticksSincePermHeal = 0;
                if (!ModSettings.onlyHealPermWhenGifted || ModSettings.onlyHealPermWhenGifted && IsGifted(Pawn)) HealPermanent();
            }

            ticksSortedArray++;
            ticksSinceStatusCheck++;
            ticksSincePsyCost++;
            ticksSincePermHeal++;
            ticksSinceHeatCheck++;
        }

        private void ReduceHeatOrCold()
        {
            var cold = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x?.def == HediffDefOf.Hypothermia);
            ReduceSeverity(cold);
            var heat = Pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x?.def == HediffDefOf.Heatstroke);
            ReduceSeverity(heat);
        }

        public bool ReduceSeverity(Hediff hediff)
        {
            if (hediff == null || hediff.Severity < 0.3f) return false;
            Messages.Message("ProtectiveAuraHeatRegulation".Translate(Pawn.NameShortColored, parent.Label), MessageTypeDefOf.SilentInput, false);
            hediff.Severity = 0.05f;
            return true;
        }

        public bool CanShowShipProtectGizmo() =>
            CompatibilityPatches.detectedAssemblies.Any(x => x.assemblyString == Patches.Compatibility.SoS2Patch.SoS2AssemblyName)
                && Current.Game.Maps.Any(x => x?.Biome?.defName == Patches.Compatibility.SoS2Patch.OuterSpaceBiomeName && x?.ParentFaction != null && x.ParentFaction == Faction.OfPlayerSilentFail)
                && Pawn?.HasPsylink == true
                && ModSettings.sos2AuraHeatManagementEnabled;

        public bool CanProtectShip(float heatToTakeIn = 0f)
        {
            if (!CompatibilityPatches.detectedAssemblies.Any(x => x.assemblyString == Patches.Compatibility.SoS2Patch.SoS2AssemblyName
            || Pawn?.psychicEntropy?.IsPsychicallySensitive != true
            || Pawn?.psychicEntropy?.Psylink?.level == null
            || Pawn?.psychicEntropy?.Psylink?.level == 0
            || Pawn.psychicEntropy?.EntropyRelativeValue + heatToTakeIn > Pawn.psychicEntropy.MaxEntropy && Pawn.psychicEntropy.limitEntropyAmount == false
            )) return false;
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (base.CompGetGizmos() != null) foreach (var baseGiz in base.CompGetGizmos()) if (baseGiz != null) yield return baseGiz;
            if (!CanShowShipProtectGizmo())
            {
                protectsTheShip = false;
                yield break;
            }
            yield return new Command_Toggle
            {
                isActive = () => protectsTheShip,
                defaultLabel = "sos2ProtectSpaceship".Translate(),
                defaultDesc = "sos2ProtectSpaceshipDesc".Translate(),
                order = 198f,
                icon = ContentFinder<Texture2D>.Get("UI/Abilities/Protect", true),
                toggleAction = () =>
                {
                    SoundDefOf.Click.PlayOneShot(null);
                    protectsTheShip = !protectsTheShip;
                }
            };
        }

        private void CureIllnesses()
        {
            var diseases = Pawn.health.hediffSet.hediffs.Where(x => sicknessesToBeHealed.Contains(x.def.defName));
            if (diseases != null && diseases.Count() != 0)
            {
                foreach (var hediff in Pawn.health.hediffSet.hediffs.Where(x => diseases.Contains(x)).Reverse())
                {
                    Pawn.health.hediffSet.hediffs.Remove(hediff);
                    hediff.PostRemoved();
                    Pawn.health.Notify_HediffChanged(null);
                }
            }
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
            if (ModSettings.stopMentalBreaks && Pawn.InMentalState)
            {
                Pawn.MentalState.RecoverFromState();
                Messages.Message("CAProtectiveAuraCureMentalBreaks".Translate(Pawn.NameShortColored, parent.Label), MessageTypeDefOf.PositiveEvent, false);
            }
        }

        private void ExtinguishFire()
        {
            if (this.Pawn.HasAttachment(ThingDefOf.Fire))
            {
                var fire = this.Pawn.GetAttachment(ThingDefOf.Fire);
                fire.Destroy();
                Messages.Message("CAProtectiveAuraExtinguishFire".Translate(Pawn.NameShortColored, parent.Label), MessageTypeDefOf.PositiveEvent, false);
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

            if (wound.IsPermanent()) wound.Severity = 0f;
            else HealthUtility.Cure(wound);
            if (PawnUtility.ShouldSendNotificationAbout(base.Pawn)) Messages.Message("MessagePermanentWoundHealed".Translate(this.parent.LabelCap, base.Pawn.LabelShort, wound.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent, true);
        }
    }
}
