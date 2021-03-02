using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class GuardianShieldApparel : ShieldBelt
    {
		private Vector3 impactAngleVect;
		private float energy;
		private int lastAbsorbDamageTick = -9999;
		private int lastKeepDisplayTick = -9999;
		private int KeepDisplayingTicks = 1000;
		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
		private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);
		private static Material colorMat = null;
		private int radius = 2;

		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = base.Wearer;
				return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + KeepDisplayingTicks);
			}
		}

		public override void DrawWornExtras()
		{
			if (this.ShieldState == ShieldState.Active && ShouldDisplay)
			{
				//float num = Mathf.Lerp(3.2f, 4.55f, this.energy);
				var num = Mathf.Lerp(radius * 2f * 1.16015625f, (radius + 1) * 2f * 1.16015625f, this.energy);
				Vector3 vector = base.Wearer.Drawer.DrawPos;
				vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
				if (num2 < 8)
				{
					float num3 = (float)(8 - num2) / 8f * 0.05f;
					vector += this.impactAngleVect * num3;
					num -= num3;
				}
				float angle = (float)Rand.Range(0, 360);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				colorMat = colorMat ?? MaterialPool.MatFrom((Texture2D)ForceFieldMat.mainTexture, ShaderDatabase.MoteGlow, UnityEngine.Color.red);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                //matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(radius * 2f * 1.16015625f, 1f, radius * 2f * 1.16015625f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, colorMat, 0);
			}
		}

		public void KeepDisplaying()
		{
			lastKeepDisplayTick = Find.TickManager.TicksGame;
		}
	}
}
