using Verse.AI.Group;

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
