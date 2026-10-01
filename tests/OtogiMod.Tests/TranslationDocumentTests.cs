using System.Text.Json.Nodes;
using OtogiMod.Services;
using Xunit;

namespace OtogiMod.Tests;

public sealed class TranslationDocumentTests
{
    [Theory]
    [InlineData("https://example.test/api/MScenes/123?cache=1", "MScenes")]
    [InlineData("https://example.test/api/episode/monsters/123", "character")]
    [InlineData("https://example.test/api/Episode/WorldStories", "worldstory")]
    [InlineData("https://example.test/api/UAdventures/SideStories", "sidestory")]
    [InlineData("https://example.test/api/now", null)]
    public void RoutesOnlySupportedResponses(string url, string? expected) =>
        Assert.Equal(expected, TranslationDocument.GetType(url));

    [Fact]
    public void AdultTranslationChangesOnlyDisplayFields()
    {
        string result = TranslationDocument.Apply(
            """{"MAdultDetails":[{"Name":"Alice","Serif":"hello","Id":"hello"}],"Other":"hello"}""",
            """{"Alice":"A","hello":"H"}""",
            "MAdults"
        );
        JsonNode data = JsonNode.Parse(result)!;
        Assert.Equal("A", data["MAdultDetails"]![0]!["Name"]!.GetValue<string>());
        Assert.Equal("H", data["MAdultDetails"]![0]!["Serif"]!.GetValue<string>());
        Assert.Equal("hello", data["MAdultDetails"]![0]!["Id"]!.GetValue<string>());
        Assert.Equal("hello", data["Other"]!.GetValue<string>());
    }

    [Fact]
    public void NormalStoryNormalizesLiteralNewlineMarkerAndObjectIntoArray()
    {
        string result = TranslationDocument.Apply(
            """{"Title":"title","UseRichScene":false,"MSceneDetails":[{"Phrase":"line\\nnext"}]}""",
            """{"title":"T","line next":"L"}""",
            "MScenes"
        );
        JsonArray data = (JsonArray)JsonNode.Parse(result)!;
        Assert.Equal("T", data[0]!["Title"]!.GetValue<string>());
        Assert.Equal("L", data[0]!["MSceneDetails"]![0]!["Phrase"]!.GetValue<string>());
    }

    [Fact]
    public void RichStoryTranslatesChoicesButNotUnrelatedFields()
    {
        string result = TranslationDocument.Apply(
            """[{"Title":"title","UseRichScene":true,"MRichSceneDetails":[{"Name":"Alice","Serif":"hello","Choices":[{"ChoicesDescription":"yes","Code":"yes"}]}]}]""",
            """{"title":"T","Alice":"A","hello":"H","yes":"Y"}""",
            "Mstory"
        );
        JsonNode detail = JsonNode.Parse(result)![0]!["MRichSceneDetails"]![0]!;
        Assert.Equal("H", detail["Serif"]!.GetValue<string>());
        Assert.Equal("Y", detail["Choices"]![0]!["ChoicesDescription"]!.GetValue<string>());
        Assert.Equal("yes", detail["Choices"]![0]!["Code"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("character", """{"Episodes":[{"MSceneId":123,"Title":"title"}]}""", "MScenes")]
    [InlineData("worldstory", """[{"MStoryId":123,"Title":"title"}]""", "Mstory")]
    [InlineData("sidestory", """[{"Adventures":[{"MSceneId":123,"Name":"title"}]}]""", "MScenes")]
    public void ListingsUseStoryIdsInsteadOfDownloadingListingDirectories(
        string type,
        string original,
        string expectedType
    )
    {
        string? result = TranslationDocument.MarkListing(
            original,
            type,
            (resourceType, id) =>
            {
                Assert.Equal(expectedType, resourceType);
                Assert.Equal("123", id);
                return true;
            }
        );
        Assert.Contains("title", result!);
        Assert.NotEqual(original, result);
        Assert.Null(TranslationDocument.MarkListing(original, type, (_, _) => false));
    }

    [Fact]
    public void EmptyListingDoesNotThrow() =>
        Assert.Null(TranslationDocument.MarkListing("[]", "worldstory", (_, _) => true));
}
