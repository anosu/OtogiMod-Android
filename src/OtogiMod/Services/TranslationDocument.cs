using System;
using System.Text.Json.Nodes;

namespace OtogiMod.Services;

internal static class TranslationDocument
{
    internal static string? GetType(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            return null;
        string path = uri.AbsolutePath;
        if (path.Contains("/api/MAdults/MonsterMAdults/", StringComparison.Ordinal))
            return "MAdults";
        if (path.Contains("/api/MScenes/", StringComparison.Ordinal))
            return "MScenes";
        if (path.Contains("/api/Episode/MStory/", StringComparison.Ordinal))
            return "Mstory";
        if (
            path.Contains("/api/episode/monsters", StringComparison.Ordinal)
            || path.Contains("/api/episode/spirits", StringComparison.Ordinal)
        )
            return "character";
        if (path.Contains("/api/Episode/WorldStories", StringComparison.Ordinal))
            return "worldstory";
        if (path.Contains("/api/UAdventures/SideStories", StringComparison.Ordinal))
            return "sidestory";
        return null;
    }

    internal static string Apply(string original, string translated, string type)
    {
        JsonNode data = JsonNode.Parse(original)!;
        var table = (JsonObject)JsonNode.Parse(translated)!;
        if (type == "MAdults")
        {
            foreach (JsonNode? detail in (JsonArray)data["MAdultDetails"]!)
            {
                ReplaceField(detail!, "Name", table);
                ReplaceField(detail!, "Serif", table);
            }
        }
        else
        {
            // The reference normalizes single-story responses into a story array.
            var stories = data as JsonArray ?? new JsonArray(data);
            foreach (JsonNode? story in stories)
            {
                ReplaceField(story!, "Title", table);
                bool rich = story!["UseRichScene"]?.GetValue<bool>() == true;
                foreach (
                    JsonNode? detail in (JsonArray)
                        story[rich ? "MRichSceneDetails" : "MSceneDetails"]!
                )
                {
                    ReplaceField(detail!, "Name", table);
                    ReplaceField(detail!, rich ? "Serif" : "Phrase", table);
                    if (rich)
                        foreach (JsonNode? choice in (JsonArray)detail!["Choices"]!)
                            ReplaceField(choice!, "ChoicesDescription", table);
                }
            }
            data = stories;
        }
        return data.ToJsonString();
    }

    internal static string? MarkListing(
        string original,
        string type,
        Func<string, string, bool> exists
    )
    {
        JsonNode data = JsonNode.Parse(original)!;
        JsonNode? item = type switch
        {
            "character" => First(data["Episodes"]),
            "worldstory" => First(data),
            "sidestory" => First(First(data)?["Adventures"]),
            _ => null,
        };
        if (item == null)
            return null;
        string idKey = type == "worldstory" ? "MStoryId" : "MSceneId";
        string? id = item[idKey]?.ToString();
        if (string.IsNullOrEmpty(id) || !exists(type == "worldstory" ? "Mstory" : "MScenes", id))
            return null;
        string field = type == "sidestory" ? "Name" : "Title";
        string title = "[中] " + item[field]!.GetValue<string>();
        if (type == "character" && title.Length > 16)
            title = title[..14] + "...";
        item[field] = title;
        return data.ToJsonString();
    }

    private static JsonNode? First(JsonNode? node) =>
        node is JsonArray { Count: > 0 } array ? array[0] : null;

    private static void ReplaceField(JsonNode item, string field, JsonObject table)
    {
        string? text = item[field]?.GetValue<string>();
        if (string.IsNullOrEmpty(text))
            return;
        string formatted = text.Replace("%user_name", "人間さん")
            .Replace("人間さん先生", "人間さん")
            .Replace("人間さんさん", "人間さん")
            .Replace("\\n", " ");
        string? replacement = table[formatted]?.GetValue<string>();
        if (string.IsNullOrEmpty(replacement))
            replacement = table[text]?.GetValue<string>();
        if (!string.IsNullOrEmpty(replacement))
            item[field] = replacement;
    }
}
