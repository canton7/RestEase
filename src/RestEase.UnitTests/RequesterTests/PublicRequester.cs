using RestEase;
using RestEase.Implementation;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestEase.UnitTests.RequesterTests
{
    public class PublicRequester : Requester
    {
        public PublicRequester(HttpClient httpClient)
             : base(httpClient)
        { }

        public new Uri ConstructUri(string basePath, string relativePath, IRequestInfo requestInfo)
        {
            return base.ConstructUri(basePath, relativePath, requestInfo);
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
