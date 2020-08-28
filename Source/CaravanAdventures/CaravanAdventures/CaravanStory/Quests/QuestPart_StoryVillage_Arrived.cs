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
        public override string DescriptionPart => $"You arrived!\n See how {Girl.NameShortColored} is doing.";
    }
}
