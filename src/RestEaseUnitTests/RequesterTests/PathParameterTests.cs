using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.RequesterTests
{
    public class PathParameterTests
    {
        private readonly PublicRequester requester = new PublicRequester(null);

        [Fact]
        public void SubstitutesPathParameters()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{baz}");
            requestInfo.AddPathParameter("bar", "yay");
            requestInfo.AddPathParameter("baz", "woo");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/woo", uri);
        }

        [Fact]
        public void SubstitutesMultiplePathParametersOfTheSameType()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{bar}");
            requestInfo.AddPathParameter("bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/yay", uri);
        }

        [Fact]
        public void TreatsNullPathParamsAsEmpty()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<int?>("bar", null);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo//baz", uri);
        }

        [Fact]
        public void EncodesUsingPathEncoding()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<string>("bar", "a ?b/c");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a%20%3fb%2fc/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathProperty("bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathParamInPreferenceToPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter("bar", "woo");
            requestInfo.AddPathProperty("bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/woo/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void FormatsPathParam()
        {
            var guid = Guid.NewGuid();

            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar:N}/baz");
            requestInfo.AddPathParameter("bar", guid);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/" + guid.ToString("N") + "/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void FormatsPathProperty()
        {
            var guid = Guid.NewGuid();

            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar:N}/baz");
            requestInfo.AddPathProperty("bar", guid);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/" + guid.ToString("N") + "/baz", uri, ignoreCase: true);
        }
    }
}
