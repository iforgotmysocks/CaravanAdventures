﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffComp_AncientGift : HediffComp
    {
        private int ticks = 0;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticks >= 60)
            {
                ticks = 0;
                Pawn.psychicEntropy.OffsetPsyfocusDirectly(ModSettings.ancientGiftPassivePsyfocusGainPerSec);
            }
            ticks++;
        }

        public static double AttackSpeedInBonusPercent => Math.Round(100 / (ModSettings.attackspeedMultiplier), 0);
        public static double PsyfocusRegInPercentPerHour => Math.Round(ModSettings.ancientGiftPassivePsyfocusGainPerSec * 100 * 40f, 2);
        public override string CompTipStringExtra => $"Melee attack speed: x{AttackSpeedInBonusPercent}%"
            + $"\nPsyfocus regeneration: {PsyfocusRegInPercentPerHour}% / h";

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }
    }
}
