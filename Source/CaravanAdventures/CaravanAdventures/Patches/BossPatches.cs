using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class BossPatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            //if (!ModSettings.caravanCampEnabled) return;
            var carTravelOrg = AccessTools.Method(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.PostDraw));
            var carTravelPre = new HarmonyMethod(typeof(BossPatches).GetMethod(nameof(PostDrawPrefix)));
            harmony.Patch(carTravelOrg, carTravelPre, null);
        }

		public static bool PostDrawPrefix(CompProjectileInterceptor __instance, float ___lastInterceptAngle, Material ___ForceFieldMat, Material ___ForceFieldConeMat, MaterialPropertyBlock ___MatPropertyBlock, Color ___InactiveColor)
        {
            if (__instance.parent.def.defName != "CAGuardianShieldPet") return true;

			Vector3 pos = ((CaravanStory.MechChips.Abilities.GuardianShieldPet)__instance.parent).Owner.DrawPos;
			pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			float currentAlpha = 1;
			if (currentAlpha > 0f)
			{
				Color value;
				if (__instance.Active || !Find.Selector.IsSelected(__instance.parent))
				{
					value = __instance.Props.color;
				}
				else
				{
					value = ___InactiveColor;
				}
				value.a *= currentAlpha;
				___MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(pos, Quaternion.identity, new Vector3(__instance.Props.radius * 2f * 1.16015625f, 1f, __instance.Props.radius * 2f * 1.16015625f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, ___ForceFieldMat, 0, null, 0, ___MatPropertyBlock);
			}
			float currentConeAlpha_RecentlyIntercepted = 1;
			if (currentConeAlpha_RecentlyIntercepted > 0f)
			{
				Color color = __instance.Props.color;
				color.a *= currentConeAlpha_RecentlyIntercepted;
				___MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
				Matrix4x4 matrix2 = default(Matrix4x4);
				matrix2.SetTRS(pos, Quaternion.Euler(0f, ___lastInterceptAngle - 90f, 0f), new Vector3(__instance.Props.radius * 2f * 1.16015625f, 1f, __instance.Props.radius * 2f * 1.16015625f));
				Graphics.DrawMesh(MeshPool.plane10, matrix2, ___ForceFieldConeMat, 0, null, 0, ___MatPropertyBlock);
			}

			return false;
        }


    }
}