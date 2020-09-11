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
        internal  QuestCont_Village Village { get => village; set => village = value; }
        private QuestCont_FriendlyCaravan friendlyCaravan;
        internal QuestCont_FriendlyCaravan FriendlyCaravan { get => friendlyCaravan; set => friendlyCaravan = value; }


        public void ExposeData()
        {
            Scribe_Deep.Look(ref village, "village");
            Scribe_Deep.Look(ref friendlyCaravan, "friendlyCaravan");
        }
    }
}
