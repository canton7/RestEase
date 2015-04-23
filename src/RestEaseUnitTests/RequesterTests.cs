using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests
{
    public class RequesterTests
    {
        private class MyRequester : Requester
        {
            public MyRequester(HttpClient httpClient)
                : base(httpClient)
            { }

            public new Uri ConstructUri(RequestInfo requestInfo)
            {
                return base.ConstructUri(requestInfo);
            }
        }

        private readonly MyRequester requester;

        public RequesterTests()
        {
            this.requester = new MyRequester(null);
        }

        [Fact]
        public void ConstructsUriWithNoRelativePath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null, CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathNoLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo/bar/baz", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo/bar/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathWithLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/bar/baz", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo/bar/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedPath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/ba  r/baz", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo/ba%20%20r/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo?bar=baz", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo", CancellationToken.None);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriCombiningExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo?a=yay", CancellationToken.None);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?a=yay&bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo?bar=b az", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=b+az", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo", CancellationToken.None);
            requestInfo.AddQueryParameter("b ar", "b az");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?b+ar=b+az", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo?bar=baz&bar=baz2", CancellationToken.None);
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo", CancellationToken.None);
            requestInfo.AddQueryParameter("bar", "baz");
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo?bar=baz", CancellationToken.None);
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }
    }
}
