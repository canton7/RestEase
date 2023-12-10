using System;
using System.Net.Http;
using System.Threading.Tasks;
using RestEase;
using RestEase.Implementation;

namespace RestEase.UnitTests.RequesterTests
{
    public class PublicRequester : Requester
    {
        public PublicRequester(HttpClient httpClient)
             : base(httpClient)
        { }

        public Uri ConstructUri(string relativePath, IRequestInfo requestInfo)
        {
            return base.ConstructUri(null, null, relativePath, requestInfo);
        }

        public new Uri ConstructUri(string baseAddress, string basePath, string relativePath, IRequestInfo requestInfo)
        {
            return base.ConstructUri(baseAddress, basePath, relativePath, requestInfo);
        }

        public new string SubstitutePathParameters(string path, IRequestInfo requestInfo)
        {
            return base.SubstitutePathParameters(path, requestInfo);
        }

        public new HttpContent ConstructContent(IRequestInfo requestInfo)
        {
            return base.ConstructContent(requestInfo);
        }

        public new void ApplyHeaders(IRequestInfo requestInfo, HttpRequestMessage requestMessage)
        {
            base.ApplyHeaders(requestInfo, requestMessage);
        }

        public new Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo, bool readBody)
        {
            return base.SendRequestAsync(requestInfo, readBody);
        }
    }
}
