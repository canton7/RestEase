using Moq;
using RestEase;
using System;
using System.Collections.Generic;
using System.IO;
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

            public new HttpContent ConstructContent(RequestInfo requestInfo)
            {
                return base.ConstructContent(requestInfo);
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

        [Fact]
        public void SubstitutesPathParameters()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{baz}", CancellationToken.None);
            requestInfo.AddPathParameter("bar", "yay");
            requestInfo.AddPathParameter("baz", "woo");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo/yay/woo", UriKind.Relative), uri);
        }

        [Fact]
        public void SubstitutesMultiplePathParametersOfTheSameType()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{bar}", CancellationToken.None);
            requestInfo.AddPathParameter("bar", "yay");
            var uri = this.requester.ConstructUri(requestInfo);
            Assert.Equal(new Uri("/foo/yay/yay", UriKind.Relative), uri);
        }

        [Fact]
        public void SetsContentNullIfBodyParameterInfoNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            // Not calling SetBodyParameterInfo
            var content = this.requester.ConstructContent(requestInfo);
            Assert.Null(content);
        }

        [Fact]
        public void SetsContentNullIfBodyValueIsNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, null);
            var content = this.requester.ConstructContent(requestInfo);
            Assert.Null(content);
        }

        [Fact]
        public void SetsContentAsStreamContentIfBodyIsStream()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, new MemoryStream());
            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<StreamContent>(content);
        }

        [Fact]
        public void SetsContentAsStringContentIfBodyIsString()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, "hello");
            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<StringContent>(content);
        }

        [Fact]
        public void UsesBodySerializerIfContentBodyIsObjectAndMethodIsSerialized()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            var body = new object();
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, body);

            var bodySerializer = new Mock<IRequestBodySerializer>();
            this.requester.RequestBodySerializer = bodySerializer.Object;

            bodySerializer.Setup(x => x.SerializeBody(body)).Returns("test").Verifiable();
            var content = this.requester.ConstructContent(requestInfo);

            bodySerializer.Verify();
            Assert.IsType<StringContent>(content);
        }

        [Fact]
        public void ThrowsIfBodyIsUrlEncodedAndBodyDoesNotImplementIDictionary()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            var body = new object();
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.UrlEncoded, body);

            Assert.Throws<ArgumentException>(() => this.requester.ConstructContent(requestInfo));
        }

        [Fact]
        public void UsesFormUrlEncodedSerializerIfBodyIsObjectAndMethodIsUrlEncoded()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            var body = new Dictionary<string, object>()
            {
                { "foo", "bar woo" },
                { "many", new List<string>() { "one", "two" } },
            };
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.UrlEncoded, body);

            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<FormUrlEncodedContent>(content);
            var encodedContent = ((FormUrlEncodedContent)content).ReadAsStringAsync().Result;
            Assert.Equal("foo=bar+woo&many=one&many=two", encodedContent);
        }

        [Fact]
        public void ThrowsIfBodySerializationMethodIsUnknown()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.SetBodyParameterInfo((BodySerializationMethod)100, new object());

            Assert.Throws<InvalidOperationException>(() => this.requester.ConstructContent(requestInfo));
        }
    }
}
