namespace OtogiMod;

internal static class Config
{
    internal static TestEntry TranslationFontUrl { get; } = new("https://fonts.example.test/font");
    internal static TestEntry TranslationCdn { get; } = new("https://cdn.example.test/");
}

internal sealed record TestEntry(string Value);

internal static class Logger
{
    internal static void Warn(string message) { }
}
