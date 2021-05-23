using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestNode_Temp : QuestNode
    {
        protected override void RunInt()
        {
            var quest = QuestGen.quest;
            if (!StartQuest()) return;
        }

        private bool StartQuest() => true;
        protected override bool TestRunInt(Slate slate) => true;
    }
}
