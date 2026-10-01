using System.Collections.Generic;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using Utility.Notifications;

namespace OtogiMod;

/// <summary>Editable translation configuration loaded from MelonPreferences.</summary>
public static class Config
{
    public static readonly string FilePath = Path.Combine(
        MelonEnvironment.UserDataDirectory,
        $"{ModInfo.Name}.cfg"
    );

    public static MelonPreferences_Entry<string> TranslationFontUrl { get; private set; } = null!;
    public static MelonPreferences_Entry<string> TranslationCdn { get; private set; } = null!;

    private static bool _initializing;
    private static bool _entriesBound;
    private static MelonPreferences_Category _preferenceCategory = null!;
    private static readonly List<MelonPreferences_Category> PreferenceCategories = new();

    /// <summary>Loads and persists the configured translation endpoints.</summary>
    public static void Initialize()
    {
        _initializing = true;
        try
        {
            if (!_entriesBound)
            {
                BindAllEntries();
                _entriesBound = true;
            }

            _preferenceCategory.LoadFromFile(false);
            foreach (var category in PreferenceCategories)
                category.SaveToFile(false);
        }
        finally
        {
            _initializing = false;
        }
    }

    private static void BindAllEntries()
    {
        var translation = CreateCategory($"{ModInfo.Name}.Translation");
        TranslationFontUrl = CreateEntry(
            translation,
            "FontURL",
            "https://assets.ntr.best/android/fonts/otogi-font",
            "翻译字体资源地址"
        );
        TranslationCdn = CreateEntry(
            translation,
            "CDN",
            "https://raw.githubusercontent.com/alex343425/otogitranslate/refs/heads/main",
            "翻译资源 CDN；修改后重启生效"
        );
    }

    private static MelonPreferences_Category CreateCategory(string name)
    {
        var category = MelonPreferences.CreateCategory(name);
        category.SetFilePath(FilePath, false, false);
        if (PreferenceCategories.Count == 0)
            _preferenceCategory = category;
        PreferenceCategories.Add(category);
        return category;
    }

    private static MelonPreferences_Entry<T> CreateEntry<T>(
        MelonPreferences_Category category,
        string key,
        T defaultValue,
        string description
    )
    {
        var entry = category.CreateEntry(key, defaultValue, description, description);
        entry.OnEntryValueChanged.Subscribe(
            (_, newValue) =>
            {
                if (_initializing)
                    return;

                category.SaveToFile(false);
                Logger.Info($"[{category.Identifier}] {key} => {newValue}");
                Toast.Info($"[{category.Identifier}]", $"{key} => {newValue}");
            }
        );
        return entry;
    }
}
