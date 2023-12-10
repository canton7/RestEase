using System;
using System.Net.Http;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEase.UnitTests.RequesterTests
{
    public class UriConstructionTests
    {
        private readonly PublicRequester requester = new(new HttpClient() { BaseAddress = new Uri("http://client.base.address/base") });

        [Fact]
        public void ConstructsUriWithEscapedPath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo/ba  r/baz", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo/ba%20%20r/baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriCombiningExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            var uri = this.requester.ConstructUri("/foo?a=yay", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?a=yay&bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=b az", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "b ar", "b az");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?b+ar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz&bar=baz2", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz");
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz2");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", "baz2");
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://client.base.address/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ThrowsIfUriIsUnparsable()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "http://base.com/");
            Assert.Throws<FormatException>(() => this.requester.ConstructUri("http://api.com:80:80/foo", requestInfo));
        }

        [Fact]
        public void ThrowsWithNullBaseAddressAndNullPath()
        {
            var requester = new PublicRequester(new HttpClient() { BaseAddress = null });
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            Assert.Throws<FormatException>(() => requester.ConstructUri(null, requestInfo));
        }

        [Fact]
        public void AllowsNullBaseAddressAndNonNullPath()
        {
            var requester = new PublicRequester(new HttpClient() { BaseAddress = null });
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = requester.ConstructUri("foo", requestInfo);
            Assert.Equal("http://foo/", uri.ToString());
        }

        [Theory]
        [InlineData("http://client.base.address/base", null, null, null, "http://client.base.address/base")]
        [InlineData("http://client.base.address/base", null, null, "foo/bar/baz", "http://client.base.address/base/foo/bar/baz")]
        [InlineData("http://client.base.address/base", null, null, "/foo/bar/baz", "http://client.base.address/base/foo/bar/baz")]
        [InlineData("http://client.base.address/base", null, null, "http://example.com/foo/bar", "http://example.com/foo/bar")]

        [InlineData(null, "http://base.address/base", null, null, "http://base.address/base")]
        [InlineData(null, "http://base.address/base", null, "foo/bar/baz", "http://base.address/base/foo/bar/baz")]
        [InlineData(null, "http://base.address/base", null, "/foo/bar/baz", "http://base.address/base/foo/bar/baz")]
        [InlineData(null, "http://base.address/base", null, "http://example.com/foo/bar", "http://example.com/foo/bar")]

        [InlineData("http://client.base.address/base", "http://base.address/base", null, null, "http://client.base.address/base")]
        [InlineData("http://client.base.address/base", "http://base.address/base", null, "foo/bar/baz", "http://client.base.address/base/foo/bar/baz")]
        [InlineData("http://client.base.address/base", "http://base.address/base", null, "/foo/bar/baz", "http://client.base.address/base/foo/bar/baz")]
        [InlineData("http://client.base.address/base", "http://base.address/base", null, "http://example.com/foo/bar", "http://example.com/foo/bar")]

        // ---

        [InlineData("http://client.base.address/base", null, "basePath", null, "http://client.base.address/base/basePath")]
        [InlineData("http://client.base.address/base", null, "/basePath", null, "http://client.base.address/base/basePath")]
        [InlineData("http://client.base.address/base", null, "/basePath/", null, "http://client.base.address/base/basePath/")]
        [InlineData("http://client.base.address/base", null, "http://example.com/basePath", null, "http://example.com/basePath")]

        [InlineData(null, "http://base.address/base", "basePath", null, "http://base.address/base/basePath")]
        [InlineData(null, "http://base.address/base", "/basePath", null, "http://base.address/base/basePath")]
        [InlineData(null, "http://base.address/base", "/basePath/", null, "http://base.address/base/basePath/")]
        [InlineData(null, "http://base.address/base", "http://example.com/basePath", null, "http://example.com/basePath")]

        // ---

        [InlineData("http://client.base.address/base", null, "basePath", "path", "http://client.base.address/base/basePath/path")]
        [InlineData("http://client.base.address/base", null, "/basePath", "path", "http://client.base.address/base/basePath/path")]
        [InlineData("http://client.base.address/base", null, "basePath/", "path", "http://client.base.address/base/basePath/path")]
        [InlineData("http://client.base.address/base", null, "basePath", "/path", "http://client.base.address/base/path")]
        [InlineData("http://client.base.address/base", null, "basePath/", "/path", "http://client.base.address/base/path")]
        [InlineData("http://client.base.address/base", null, "basePath/", "/path/", "http://client.base.address/base/path/")]

        [InlineData(null, "http://base.address/base", "basePath", "path", "http://base.address/base/basePath/path")]
        [InlineData(null, "http://base.address/base", "/basePath", "path", "http://base.address/base/basePath/path")]
        [InlineData(null, "http://base.address/base", "basePath/", "path", "http://base.address/base/basePath/path")]
        [InlineData(null, "http://base.address/base", "basePath", "/path", "http://base.address/base/path")]
        [InlineData(null, "http://base.address/base", "basePath/", "/path", "http://base.address/base/path")]
        [InlineData(null, "http://base.address/base", "basePath/", "/path/", "http://base.address/base/path/")]

        // ---

        [InlineData("http://client.base.address/base", null, "basePath/", "http://example.com/foo", "http://example.com/foo")]
        [InlineData("http://client.base.address/base", null, "http://test.example.com/basePath", "http://example.com/foo", "http://example.com/foo")]

        [InlineData(null, "http://base.address/base", "basePath/", "http://example.com/foo", "http://example.com/foo")]
        [InlineData(null, "http://base.address/base", "http://test.example.com/basePath", "http://example.com/foo", "http://example.com/foo")]

        public void CombinesUriParts(string httpClientBaseAddress, string baseAddress, string basePath, string path, string expected)
        {
            var requester = new PublicRequester(new HttpClient()
            {
                BaseAddress = httpClientBaseAddress == null ? null : new Uri(httpClientBaseAddress),
            });
            var requestInfo = new RequestInfo(HttpMethod.Get, path) { BasePath = basePath };
            var uri = requester.ConstructUri(baseAddress, basePath, path, requestInfo);
            Assert.Equal(expected, uri.ToString());
        }
    }
}
