using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace OtogiMod.Services;

internal static class Translation
{
    internal static string FontUrl => Config.TranslationFontUrl.Value;
    private static readonly ConcurrentDictionary<string, string> Cache = new();
    private static readonly ConcurrentDictionary<string, bool> CheckCache = new();
    private static HttpClient? _client;
    private static string _cdn = string.Empty;

    internal static void Initialize()
    {
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _cdn = Config.TranslationCdn.Value.TrimEnd('/');
        Cache.Clear();
        CheckCache.Clear();
    }

    internal static void Shutdown()
    {
        _client?.Dispose();
        _client = null;
        Cache.Clear();
        CheckCache.Clear();
    }

    internal static string? Translate(string url, string data)
    {
        if (_client == null || string.IsNullOrWhiteSpace(data))
            return null;
        string? type = TranslationDocument.GetType(url);
        if (type == null)
            return null;
        if (type is "character" or "worldstory" or "sidestory")
            return TranslationDocument.MarkListing(data, type, CheckTranslation);

        string id = new Uri(url).AbsolutePath.Split('/')[^1];
        string source = Cache.GetOrAdd($"{type}-{id}", _ => Download(type, id));
        return source.Length == 0 ? null : TranslationDocument.Apply(data, source, type);
    }

    private static bool CheckTranslation(string type, string id) =>
        CheckCache.GetOrAdd(
            $"{type}-{id}",
            _ =>
            {
                try
                {
                    using var request = new HttpRequestMessage(
                        HttpMethod.Head,
                        $"{_cdn}/{type}/{id}_gb.json?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                    );
                    using var response = _client!.SendAsync(request).GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode;
                }
                catch (Exception exception)
                {
                    Logger.Warn(
                        $"Translation availability check failed [{type}/{id}]: {exception.Message}"
                    );
                    return false;
                }
            }
        );

    private static string Download(string type, string id)
    {
        return Fetch($"{_cdn}/{type}/{id}_gb.json") ?? string.Empty;
    }

    private static string? Fetch(string url)
    {
        try
        {
            using var response = _client!
                .GetAsync($"{url}?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}")
                .GetAwaiter()
                .GetResult();
            return response.IsSuccessStatusCode
                ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                : null;
        }
        catch (Exception exception)
        {
            Logger.Warn($"Translation download failed: {exception.Message}");
            return null;
        }
    }
}
