using System;
using HarmonyLib;
using Il2CppBestHTTP;

namespace OtogiMod.Patches;

[HarmonyPatch]
internal static class TranslationPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(HTTPManager),
        nameof(HTTPManager.SendRequest),
        new[] { typeof(HTTPRequest) }
    )]
    private static void RewriteFont(HTTPRequest request)
    {
        // Patching an allocating interop constructor can leave the native request uninitialized.
        try
        {
            if (
                (request?.Uri?.ToString() ?? string.Empty).EndsWith(
                    "Assets/font",
                    StringComparison.Ordinal
                )
            )
                request!.Uri = new Il2CppSystem.Uri(Services.Translation.FontUrl);
        }
        catch (Exception exception)
        {
            Logger.Warn($"Font redirect skipped: {exception.Message}");
        }
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
