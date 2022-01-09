using RimWorld;
using System;
using Verse;

namespace CaravanAdventures.CaravanStory
{
    class GameCondition_Apocalypse : GameCondition
    {
        private float tempOffset;
        public float TempOffset { get => tempOffset; set => tempOffset = value; }
        private int ticks;
        private bool active = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tempOffset, "tempOffset", 0);
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref active, "active", false);
        }

        public override string TooltipString
        {
			get
			{
				string text = this.def.LabelCap;
				if (this.Permanent)
				{
					text += "\n" + "Permanent".Translate().CapitalizeFirst();
				}
				
				text += "\n";
				text = text + "\n" + string.Format(this.Description, Math.Round(tempOffset, 1));
				return text;
			}
		}

        public override void GameConditionTick()
        {
            base.GameConditionTick();

            if (ticks >= 60000)
            {
                TempOffset += ModSettings.apocalypseTemperatureChangePerDay;
                ticks = 0;
            }
            ticks++;
        }

        public override float TemperatureOffset()
        {
            return TempOffset;
        }

    }
}
