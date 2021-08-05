using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace CaravanAdventures.Patches
{
    internal class BossPatches
    {
        public static void ApplyPatches()
        {
            var projDrawOrg = AccessTools.Method(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.PostDraw));
            var projDrawPre = new HarmonyMethod(typeof(BossPatches).GetMethod(nameof(PostDrawPrefix)));
            HarmonyPatcher.harmony.Patch(projDrawOrg, projDrawPre, null);

            var projOrg = AccessTools.Method(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.CheckIntercept));
            var projPost = new HarmonyMethod(typeof(BossPatches).GetMethod(nameof(CheckInterceptPostFix)));
            HarmonyPatcher.harmony.Patch(projOrg, null, projPost);
        }

        public static bool PostDrawPrefix(CompProjectileInterceptor __instance, float ___lastInterceptAngle, Material ___ForceFieldMat, Material ___ForceFieldConeMat, MaterialPropertyBlock ___MatPropertyBlock, Color ___InactiveColor)
        {
            if (__instance.parent.def.defName != "CAGuardianShield") return true;

            Vector3 pos = ((CaravanStory.MechChips.Abilities.GuardianShield)__instance.parent).Owner.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float currentAlpha = 1; //todo grab orig. method
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
            float currentConeAlpha_RecentlyIntercepted = 1; // todo grab orig. method
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

        public static void CheckInterceptPostFix(ref bool __result, CompProjectileInterceptor __instance, Projectile projectile)
        {
            if (!__result || projectile == null) return;
            if (__instance.parent.def.defName != "CAGuardianShield") return;
            var parent = __instance.parent as CaravanStory.MechChips.Abilities.GuardianShield;
            if (parent == null) return;
            parent.AbsorbedDamage += projectile.DamageAmount;
        }


    }
}