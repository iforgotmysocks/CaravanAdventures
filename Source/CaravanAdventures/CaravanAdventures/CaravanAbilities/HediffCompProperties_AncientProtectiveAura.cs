using Verse;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffCompProperties_AncientProtectiveAura : HediffCompProperties
    {
        public HediffCompProperties_AncientProtectiveAura()
        {
            this.compClass = typeof(HediffComp_AncientProtectiveAura);
        }

        public bool linked = false;
    }
}
