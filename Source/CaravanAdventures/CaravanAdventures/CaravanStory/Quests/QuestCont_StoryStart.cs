using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_StoryStart : IExposable
    {
        private Pawn gifted;

        public Pawn Gifted { get => gifted; set => gifted = value; }

        public QuestCont_StoryStart()
        {
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref gifted, "gifted");
        }
    }
}
