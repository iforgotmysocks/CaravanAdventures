using CaravanAdventures.CaravanStory;
using RimWorld;
using System;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientGift : HediffComp
    {
        private long ticks = 0;
        private uint checkRemoveDupesTicks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticks >= 60)
            {
                ticks = 0;
                Pawn.psychicEntropy.OffsetPsyfocusDirectly(ModSettings.ancientGiftPassivePsyfocusGainPerSec);
            }

            if (ModSettings.onlyAllowOneConcurrentlyGiftedPawn && checkRemoveDupesTicks >= 2000)
            {
                checkRemoveDupesTicks = 0;
                var isStoryPawn = CompCache.StoryWC.questCont.StoryStart?.Gifted == this.parent.pawn;
                if (!isStoryPawn)
                {
                    StoryUtility.StripGiftFromPawn(this.parent.pawn);
                    Messages.Message("CaravanAdventures_AncientGiftRemoved".Translate(this.parent.pawn.Name.ToString()), this.parent.pawn, MessageTypeDefOf.NegativeEvent);
                }
            }

            ticks++;
            checkRemoveDupesTicks++;
        }

        public static float AttackSpeedMultiplier => 1f - ModSettings.attackspeedMultiplierNegated;
        public static double AttackSpeedInBonusPercent => Math.Round(100 / AttackSpeedMultiplier, 0);
        public static double PsyfocusRegInPercentPerHour => Math.Round(ModSettings.ancientGiftPassivePsyfocusGainPerSec * 100 * 40f, 2);
        public override string CompTipStringExtra => $"  - Melee attack speed: x{AttackSpeedInBonusPercent}%"
            + $"\n  - Psyfocus regeneration: {PsyfocusRegInPercentPerHour}% / h";

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }
    }
}
