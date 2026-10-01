using System;
using HarmonyLib;
using Il2CppBestHTTP;

namespace OtogiMod.Patches;

[HarmonyPatch]
internal static class TranslationPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(HTTPRequest),
        MethodType.Constructor,
        new[]
        {
            typeof(Il2CppSystem.Uri),
            typeof(HTTPMethods),
            typeof(bool),
            typeof(bool),
            typeof(OnRequestFinishedDelegate),
        }
    )]
    private static void RewriteFont(ref Il2CppSystem.Uri __0)
    {
        if ((__0?.ToString() ?? string.Empty).EndsWith("Assets/font", StringComparison.Ordinal))
            __0 = new Il2CppSystem.Uri(Services.Translation.FontUrl);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HTTPResponse), nameof(HTTPResponse.DataAsText), MethodType.Getter)]
    private static void TranslateResponse(HTTPResponse __instance, ref string __result)
    {
        try
        {
            string url = __instance.baseRequest?.Uri?.ToString() ?? string.Empty;
            string? translated = Services.Translation.Translate(url, __result);
            if (translated != null)
                __result = translated;
        }
        catch (Exception exception)
        {
            Logger.Warn($"Translation skipped: {exception.Message}");
        }
    }
}
