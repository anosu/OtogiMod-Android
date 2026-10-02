using System.Reflection;
using HarmonyLib;
using Il2CppBestHTTP;
using OtogiMod.Patches;
using Xunit;

namespace OtogiMod.Tests;

public sealed class RequestPatchTests
{
    [Fact]
    public void RedirectsFontOnTheExistingInitializedRequestBeforeSending()
    {
        var request = new HTTPRequest
        {
            Uri = new Il2CppSystem.Uri("https://game.example.test/Assets/font"),
        };
        InvokeFont(request);
        Assert.Equal(Config.TranslationFontUrl.Value, request.Uri!.ToString());
        Assert.Equal("fonts.example.test", HTTPManager.SendRequest(request));
    }

    [Theory]
    [InlineData("https://game.example.test/api/now")]
    [InlineData("https://game.example.test/Assets/font?x=1")]
    public void LeavesUnrelatedRequestUriUnchanged(string url)
    {
        var uri = new Il2CppSystem.Uri(url);
        var request = new HTTPRequest { Uri = uri };
        InvokeFont(request);
        Assert.Same(uri, request.Uri);
        Assert.Equal("game.example.test", HTTPManager.SendRequest(request));
    }

    [Fact]
    public void RedirectDoesNotTargetTheAllocatingInteropConstructor()
    {
        var target = FontPatch.GetCustomAttribute<HarmonyPatch>()!.info;
        Assert.Equal(typeof(HTTPManager), target.declaringType);
        Assert.Equal(nameof(HTTPManager.SendRequest), target.methodName);
        Assert.Equal(new[] { typeof(HTTPRequest) }, target.argumentTypes);
    }

    private static MethodInfo FontPatch =>
        typeof(TranslationPatch).GetMethod(
            "RewriteFont",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

    private static void InvokeFont(HTTPRequest request) =>
        FontPatch.Invoke(null, new object[] { request });
}
