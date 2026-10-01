using HarmonyLib;
using UnityEngine;

namespace OtogiMod.Patches;

[HarmonyPatch]
internal static class PerformancePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Setter)]
    private static void LockFrameRate(ref int value) => value = 60;
}
