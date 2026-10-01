using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OtogiMod.Services;
using Xunit;

namespace OtogiMod.Tests;

public sealed class TranslationTransportTests : IDisposable
{
    private const string Adult = """{"MAdultDetails":[{"Serif":"hello"}]}""";
    private const string Table = """{"hello":"translated"}""";

    [Fact]
    public void DownloadsOnlyConfiguredCdnAndReusesCachedTranslation()
    {
        using var handler = Install(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Table),
        });
        const string url = "https://game.example.test/api/MAdults/MonsterMAdults/123?time=1";
        string result = Translation.Translate(url, Adult)!;
        Assert.Equal(
            "translated",
            JsonNode.Parse(result)!["MAdultDetails"]![0]!["Serif"]!.GetValue<string>()
        );
        Assert.Equal(result, Translation.Translate(url, Adult));
        Assert.Collection(
            handler.Requests,
            request =>
            {
                Assert.Equal("GET", request.Method);
                Assert.Equal("cdn.example.test", request.Host);
                Assert.Equal("/MAdults/123_gb.json", request.Path);
            }
        );
    }

    [Fact]
    public void ListingChecksConfiguredCdnByHead()
    {
        using var handler = Install(_ => new HttpResponseMessage(HttpStatusCode.OK));
        string? result = Translation.Translate(
            "https://game.example.test/api/Episode/WorldStories",
            """[{"MStoryId":12,"Title":"story"}]"""
        );
        Assert.Equal("[中] story", JsonNode.Parse(result!)![0]!["Title"]!.GetValue<string>());
        Assert.Collection(
            handler.Requests,
            request =>
            {
                Assert.Equal("HEAD", request.Method);
                Assert.Equal("cdn.example.test", request.Host);
                Assert.Equal("/Mstory/12_gb.json", request.Path);
            }
        );
    }

    [Fact]
    public void MissingTranslationLeavesGameResponseUnchanged()
    {
        using var handler = Install(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        Assert.Null(
            Translation.Translate("https://game.example.test/api/MAdults/MonsterMAdults/123", Adult)
        );
        Assert.Single(handler.Requests);
    }

    private static RecordingHandler Install(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        Translation.Initialize();
        FieldInfo field = typeof(Translation).GetField(
            "_client",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;
        ((HttpClient)field.GetValue(null)!).Dispose();
        var handler = new RecordingHandler(respond);
        field.SetValue(null, new HttpClient(handler));
        return handler;
    }

    public void Dispose() => Translation.Shutdown();

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        internal List<(string Method, string Host, string Path)> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken token
        )
        {
            Requests.Add(
                (request.Method.Method, request.RequestUri!.Host, request.RequestUri.AbsolutePath)
            );
            return Task.FromResult(respond(request));
        }
    }
}
