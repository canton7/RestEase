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
    public class UriConstructionTests
    {
        private readonly PublicRequester requester = new PublicRequester(new HttpClient() { BaseAddress = new Uri("http://api.example.com/base") });

        [Fact]
        public void ConstructsUriWithNoRelativePath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base"), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathNoLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("foo/bar/baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo/bar/baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathWithLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo/bar/baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo/bar/baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedPath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo/ba  r/baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo/ba%20%20r/baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriCombiningExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri("/foo?a=yay", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?a=yay&bar=baz"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=b az", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("b ar", "b az");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?b+ar=b+az"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz&bar=baz2", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=baz&bar=baz2"), uri);
        }

        [Fact]
        public void ThrowsIfUriIsUnparsable()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "http://base.com/");
            Assert.Throws<UriFormatException>(() => this.requester.ConstructUri("http://api.com:80:80/foo", requestInfo));
        }

        [Fact]
        public void AllowsAbsoluteUri()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "http://base.com/");
            var uri = this.requester.ConstructUri("http://api.com/foo/bar", requestInfo);
            Assert.True(uri.IsAbsoluteUri);
            Assert.Equal(new Uri("http://api.com/foo/bar"), uri);
        }
    }
}
