using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.CaravanStory.Quests
{
    static class QuestCont
    {
        private static QuestCont_Village village;
        internal static QuestCont_Village Village { get => village; set => village = value; }
    }
}
