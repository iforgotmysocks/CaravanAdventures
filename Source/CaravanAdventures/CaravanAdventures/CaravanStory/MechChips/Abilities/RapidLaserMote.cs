using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class RapidLaserMote : Mote
    {
        public Vector3 launchPos;
        public Vector3 targetPos;
        public float beamSize = 0.4f;
        public Color color = Color.blue;
        public LocalTargetInfo launchObject;
        public LocalTargetInfo targetObject;
        public Shader shader = ShaderDatabase.MoteGlow;
        private Color curColor;
        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private Material bufferMat = BeamMat;

        public override void Draw()
        {
            if (launchObject == default || targetObject == default) return;
            launchPos = launchObject.Thing.DrawPos;
            targetPos = targetObject.Thing?.DrawPos ?? targetObject.Cell.ToVector3();
            launchPos.y = targetPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            if (launchPos == targetPos) return;
            if (curColor == default) curColor = color;
            curColor.a = color.a * Alpha;
            if (curColor.a != bufferMat.color.a) bufferMat = MaterialPool.MatFrom((Texture2D)BeamMat.mainTexture, shader, curColor);
            var pos = (this.targetPos + this.launchPos) / 2f;
            var rotation = Quaternion.LookRotation(this.launchPos - this.targetPos);
            var size = new Vector3(beamSize, 1f, (launchPos - targetPos).MagnitudeHorizontal());
            var matrix = default(Matrix4x4);
            matrix.SetTRS(pos, rotation, size);
            Graphics.DrawMesh(MeshPool.plane10, matrix, bufferMat, 0);
        }

    }
}
