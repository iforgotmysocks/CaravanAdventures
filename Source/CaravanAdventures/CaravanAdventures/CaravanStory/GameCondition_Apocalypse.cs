using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class GameCondition_Apocalypse : GameCondition
    {
        private float tempOffset;
        public float TempOffset { get => tempOffset; set => tempOffset = value; }
        private int ticks;
        private float anualIncrease;
        public float AnualIncrease { get => anualIncrease; set => anualIncrease = value; }
        private bool active = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tempOffset, "tempOffset", 0);
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref anualIncrease, "anualIncrease");
            Scribe_Values.Look(ref active, "active", false);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();

            if (ticks >= 60000 * 60)
            {
                TempOffset += AnualIncrease;
                ticks = 0;
            }
            ticks++;
        }

        public override int TransitionTicks
        {
            get
            {
                return 12000;
            }
        }

        public override float TemperatureOffset()
        {
            return GameConditionUtility.LerpInOutValue(this, (float)this.TransitionTicks, TempOffset);
        }

    }
}
