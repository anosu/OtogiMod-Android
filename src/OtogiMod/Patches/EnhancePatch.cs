using System;
using HarmonyLib;
using Il2CppSpine.Unity;
using UnityEngine;

namespace OtogiMod.Patches;

[HarmonyPatch]
internal static class EnhancePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtlasAsset), nameof(AtlasAsset.GetAtlas))]
    private static void ReduceMosaic(AtlasAsset __instance)
    {
        try
        {
            if (__instance.materials == null || __instance.materials.Length == 0)
                return;
            Material material = __instance.materials[0];
            if (material?.shader?.name == "Spine/SkeletonMosaic")
                material.SetFloat("_BlockSize", 0.001f);
        }
        catch (Exception exception)
        {
            Logger.Warn($"Mosaic enhancement skipped: {exception.Message}");
        }
    }
}
