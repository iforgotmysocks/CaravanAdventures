using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class CirclingBladesMote : Mote
    {
        public LocalTargetInfo launchObject;

        public override void Draw()
        {
            base.Draw();

            //    Vector3 size = new Vector3(3, 1f, 3);
            //    Matrix4x4 matrix = default(Matrix4x4);
            //    Vector3 pos = pawnDrawPos;
            //    matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), size);
            //    Graphics.DrawMesh(MeshPool.plane10, matrix, matBlade, 0);
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            this.exactRotation += this.rotationRate * deltaTime;
            this.exactPosition = launchObject.Thing.DrawPos;
        }

    }
}
