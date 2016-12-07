using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEaseUnitTests.RequesterTests
{
    public class PublicRequester : Requester
    {
        public PublicRequester(HttpClient httpClient)
             : base(httpClient)
        { }

        public new Uri ConstructUri(string relativePath, IRequestInfo requestInfo)
        {
            return base.ConstructUri(relativePath, requestInfo);
        }

        public new string SubstitutePathParameters(IRequestInfo requestInfo)
        {
            return base.SubstitutePathParameters(requestInfo);
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
