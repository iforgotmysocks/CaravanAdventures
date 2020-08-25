using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestPart_StoryVillage_Arrived : QuestPart
    {
        public Pawn Girl { get; set; }
        public string QuestTag { get; set; }

        public string inSignal_Arrived;

        public override bool QuestPartReserves(Pawn p)
        {
            return base.QuestPartReserves(p);
        }

        public override string DescriptionPart => $"You arrived!\n See how {Girl.NameShortColored} is doing.";

        public override void Cleanup()
        {
            Find.SignalManager.SendSignal(new Signal(QuestTag + ".QuestEnded", this.quest.Named("SUBJECT")));
        }
    }
}
