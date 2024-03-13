using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class CirclingBladesMote : Mote
    {
        public LocalTargetInfo launchObject;

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            this.exactRotation += this.rotationRate * deltaTime;
            this.exactPosition = launchObject.Thing.DrawPos;
        }

    }
}
