using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont : IExposable
    {
        private  QuestCont_Village village;
        private QuestCont_FriendlyCaravan friendlyCaravan;
        private QuestCont_LastJudgment lastJudgment;

        public QuestCont_Village Village { get => village; set => village = value; }
        public QuestCont_FriendlyCaravan FriendlyCaravan { get => friendlyCaravan; set => friendlyCaravan = value; }
        public QuestCont_LastJudgment LastJudgment { get => lastJudgment; set => lastJudgment = value; }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref village, "village");
            Scribe_Deep.Look(ref friendlyCaravan, "friendlyCaravan");
            Scribe_Deep.Look(ref lastJudgment, "lastJudgement");
        }

        public void EnsureFieldsInitialized()
        {
            Village = Village ?? new QuestCont_Village();
            FriendlyCaravan = FriendlyCaravan ?? new QuestCont_FriendlyCaravan();
            LastJudgment = LastJudgment ?? new QuestCont_LastJudgment();
        }
    }
}
