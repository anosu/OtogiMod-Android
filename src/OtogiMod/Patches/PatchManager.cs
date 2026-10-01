using System;
using HarmonyLib;

namespace OtogiMod.Patches;

/// <summary>Owns installation and removal of this mod's Harmony patches.</summary>
internal static class PatchManager
{
    private static HarmonyLib.Harmony? _harmony;

    /// <summary>Installs all patches once and rolls back partial installation on failure.</summary>
    internal static void Initialize()
    {
        if (_harmony != null)
            return;
        var harmony = new HarmonyLib.Harmony(ModInfo.Name);
        try
        {
            harmony.PatchAll(typeof(EnhancePatch));
            harmony.PatchAll(typeof(TranslationPatch));
            harmony.PatchAll(typeof(PerformancePatch));
            UnityEngine.Application.targetFrameRate = 60;
            _harmony = harmony;
        }
        catch
        {
            try
            {
                harmony.UnpatchSelf();
            }
            catch (Exception exception)
            {
                Logger.Error($"Patch rollback failed: {exception}");
            }
            throw;
        }
    }

    /// <summary>Removes patches owned by this mod.</summary>
    internal static void Shutdown()
    {
        var harmony = _harmony;
        _harmony = null;
        try
        {
            harmony?.UnpatchSelf();
        }
        catch (Exception exception)
        {
            Logger.Error($"Patch shutdown failed: {exception}");
        }
    }
}
