using System.Collections.Generic;
using System.Net.Http;

namespace RestEase.SourceGenerator
{
    internal static class WellKnownNames
    {
        public const string Requester = "global::RestEase.Implementation.RequestInfo";

        public static IReadOnlyDictionary<HttpMethod, string> HttpMethodProperties { get; } = new Dictionary<HttpMethod, string>()
        {
            { HttpMethod.Delete, "global::System.Net.Http.HttpMethod.Delete" },
            { HttpMethod.Get, "global::System.Net.Http.HttpMethod.Get" },
            { HttpMethod.Head, "global::System.Net.Http.HttpMethod.Head" },
            { HttpMethod.Options, "global::System.Net.Http.HttpMethod.Options" },
            { HttpMethod.Post, "global::System.Net.Http.HttpMethod.Post" },
            { HttpMethod.Put, "global::System.Net.Http.HttpMethod.Put" },
            { HttpMethod.Trace, "global::System.Net.Http.HttpMethod.Trace" },
            { PatchAttribute.PatchMethod, "global::RestEase.PatchAttribute.PatchMethod" },
        };
    }
}
