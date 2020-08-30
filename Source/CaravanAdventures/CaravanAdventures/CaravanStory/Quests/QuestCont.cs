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

        public void ExposeData()
        {
            Scribe_Deep.Look(ref village, "village");
        }
    }
}
