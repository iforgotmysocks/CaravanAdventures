using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont : IExposable
    {
        private QuestCont_StoryStart storyStart;
        private QuestCont_FriendlyCaravan friendlyCaravan;
        private QuestCont_Village village;
        private QuestCont_LastJudgment lastJudgment;

        public QuestCont_StoryStart StoryStart { get => storyStart; set => storyStart = value; }
        public QuestCont_FriendlyCaravan FriendlyCaravan { get => friendlyCaravan; set => friendlyCaravan = value; }
        public QuestCont_Village Village { get => village; set => village = value; }
        public QuestCont_LastJudgment LastJudgment { get => lastJudgment; set => lastJudgment = value; }
        
        public void ExposeData()
        {
            Scribe_Deep.Look(ref storyStart, "storyStart");
            Scribe_Deep.Look(ref friendlyCaravan, "friendlyCaravan");
            Scribe_Deep.Look(ref village, "village");
            Scribe_Deep.Look(ref lastJudgment, "lastJudgement");
        }

        public void EnsureFieldsInitialized()
        {
            StoryStart = StoryStart ?? new QuestCont_StoryStart();
            FriendlyCaravan = FriendlyCaravan ?? new QuestCont_FriendlyCaravan();
            Village = Village ?? new QuestCont_Village();
            LastJudgment = LastJudgment ?? new QuestCont_LastJudgment();
        }
    }
}
