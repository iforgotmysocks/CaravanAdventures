using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffCompProperties_OverheatingBrain : HediffCompProperties
    {
        public HediffCompProperties_OverheatingBrain()
        {
            this.compClass = typeof(HediffComp_OverheatingBrain);
        }

        public IntRange lifeTimeInSeconds;
    }
}
