using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI.Group;
using Verse;

namespace CaravanAdventures.CaravanStory.Lords
{
	class Trigger_AttackWithReinforcements : Trigger
	{
		public Trigger_AttackWithReinforcements()
		{
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return CompCache.StoryWC.storyFlags["IntroVillage_ReinforcementsArrived"];
		}
	}
}
