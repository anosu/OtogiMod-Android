namespace Il2CppSystem
{
    public sealed class Uri(string url)
    {
        private readonly System.Uri _uri = new(url);
        public string Host => _uri.Host;

        public override string ToString() => _uri.ToString();
    }
}

namespace Il2CppBestHTTP
{
    public sealed class HTTPRequest
    {
        public Il2CppSystem.Uri? Uri { get; set; }
    }

    public sealed class HTTPResponse
    {
        public HTTPRequest? baseRequest;
        public string DataAsText => string.Empty;
    }

    public static class HTTPManager
    {
        public static string SendRequest(HTTPRequest request) => request.Uri!.Host;
    }
}
