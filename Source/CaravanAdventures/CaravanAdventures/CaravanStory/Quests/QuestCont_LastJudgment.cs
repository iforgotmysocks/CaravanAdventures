using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_LastJudgment : IExposable
    {

        private GameCondition_Apocalypse apocalypse;

        public QuestCont_LastJudgment()
        {
        
        }

        public void StartApocalypse(float minTemp, float anualIncrease)
        {
            if (!ModSettings.Get().apocalypseEnabled || CompCache.StoryWC.storyFlags["Judgment_ApocalypseStarted"]) return;

                Log.Message($"Starting apocalypse");
            apocalypse = apocalypse ?? new GameCondition_Apocalypse();
            apocalypse.TempOffset = minTemp;
            apocalypse.Permanent = true;
            apocalypse.startTick = Find.TickManager.TicksGame;
            apocalypse.AnualIncrease = anualIncrease;

            CompCache.StoryWC.SetSF("Judgment_ApocalypseStarted");
        }


        public void ExposeData()
        {
            Scribe_References.Look(ref apocalypse, "apocalypse");
        }
    }
}
