using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RestEase;

namespace RestEaseUnitTests.RequesterTests
{
    public class UriConstructionTests
    {
        private readonly PublicRequester requester = new PublicRequester(new HttpClient() { BaseAddress = new Uri("http://api.example.com/base") });

        [Fact]
        public void ConstructsUriWithEscapedPath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri(null, "/foo/ba  r/baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo/ba%20%20r/baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri(null, "/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            var uri = this.requester.ConstructUri(null, "/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriCombiningExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            var uri = this.requester.ConstructUri(null, "/foo?a=yay", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?a=yay&bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri(null, "/foo?bar=b az", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "b ar", "b az");
            var uri = this.requester.ConstructUri(null, "/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?b+ar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri(null, "/foo?bar=baz&bar=baz2", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz2");
            var uri = this.requester.ConstructUri(null, "/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz2");
            var uri = this.requester.ConstructUri(null, "/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ThrowsIfUriIsUnparsable()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "http://base.com/");
            Assert.Throws<FormatException>(() => this.requester.ConstructUri(null, "http://api.com:80:80/foo", requestInfo));
        }

        [Fact]
        public void ThrowsWithNullBaseAddressAndNullPath()
        {
            var requester = new PublicRequester(new HttpClient() { BaseAddress = null });
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            Assert.Throws<FormatException>(() => requester.ConstructUri(null, null, requestInfo));
        }

        [Fact]
        public void AllowsNullBaseAddressAndNonNullPath()
        {
            var requester = new PublicRequester(new HttpClient() { BaseAddress = null });
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = requester.ConstructUri(null, "foo", requestInfo);
            Assert.Equal("http://foo/", uri.ToString());
        }

        [Theory]
        [InlineData("http://api.example.com/base", null, null, "http://api.example.com/base")]
        [InlineData("http://api.example.com/base", null, "foo/bar/baz", "http://api.example.com/base/foo/bar/baz")]
        [InlineData("http://api.example.com/base", null, "/foo/bar/baz", "http://api.example.com/base/foo/bar/baz")]
        [InlineData("http://api.example.com/base", null, "http://example.com/foo/bar", "http://example.com/foo/bar")]
        [InlineData("http://api.example.com/base", "base2", null, "http://api.example.com/base/base2")]
        [InlineData("http://api.example.com/base", "/base2", null, "http://api.example.com/base/base2")]
        [InlineData("http://api.example.com/base", "/base2/", null, "http://api.example.com/base/base2/")]
        [InlineData("http://api.example.com/base", "http://example.com/base2", null, "http://example.com/base2")]
        [InlineData("http://api.example.com/base", "base2", "path", "http://api.example.com/base/base2/path")]
        [InlineData("http://api.example.com/base", "/base2", "path", "http://api.example.com/base/base2/path")]
        [InlineData("http://api.example.com/base", "base2/", "path", "http://api.example.com/base/base2/path")]
        [InlineData("http://api.example.com/base", "base2", "/path", "http://api.example.com/base/path")]
        [InlineData("http://api.example.com/base", "base2/", "/path", "http://api.example.com/base/path")]
        [InlineData("http://api.example.com/base", "base2/", "/path/", "http://api.example.com/base/path/")]
        [InlineData("http://api.example.com/base", "base2/", "http://example.com/foo", "http://example.com/foo")]
        [InlineData("http://api.example.com/base", "http://test.example.com/base2", "http://example.com/foo", "http://example.com/foo")]
        public void CombinesUriParts(string baseAddress, string basePath, string path, string expected)
        {
            var requester = new PublicRequester(new HttpClient()
            {
                BaseAddress = baseAddress == null ? null :new Uri(baseAddress),
            });
            var requestInfo = new RequestInfo(HttpMethod.Get, path) { BasePath = basePath };
            var uri = requester.ConstructUri(basePath, path, requestInfo);
            Assert.Equal(expected, uri.ToString());
        }
    }
}
