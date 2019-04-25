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
    using RestEase;

    public class PathParameterTests
    {
        private readonly PublicRequester requester = new PublicRequester(null);

        [Fact]
        public void SubstitutesPathParameters()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{baz}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "yay");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "baz", "woo");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/woo", uri);
        }

        [Fact]
        public void SubstitutesMultiplePathParametersOfTheSameType()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{bar}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/yay", uri);
        }

        [Fact]
        public void TreatsNullPathParamsAsEmpty()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<int?>(PathSerializationMethod.ToString, "bar", null);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo//baz", uri);
        }

        [Fact]
        public void EncodesUsingPathEncoding()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<string>(PathSerializationMethod.ToString, "bar", "a ?b/cé");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a%20%3fb%2fc%c3%a9/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathParamInPreferenceToPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "woo");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/woo/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void DisablesUrlEncodingForPathPropertiesIfRequested()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{path}");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "path", "a/b+c", urlEncode: false);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a/b+c", uri, ignoreCase: true);
        }

        [Fact]
        public void DisablesUrlEncodingForPathParamsIfRequested()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{path}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "path", "a/b+c", urlEncode: false);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a/b+c", uri, ignoreCase: true);
        }
    }
}
