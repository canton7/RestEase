using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.RequesterTests
{
    public class ContentConstructionTests
    {
        private readonly PublicRequester requester = new PublicRequester(null);

        [Fact]
        public void SetsContentNullIfBodyParameterInfoNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            // Not calling SetBodyParameterInfo
            var content = this.requester.ConstructContent(requestInfo);
            Assert.Null(content);
        }

        [Fact]
        public void SetsContentNullIfBodyValueIsNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo<object>(BodySerializationMethod.Serialized, null);
            var content = this.requester.ConstructContent(requestInfo);
            Assert.Null(content);
        }

        [Fact]
        public void SetsContentAsHttpContentIfBodyIsHttpContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            var content = new StringContent("test");
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, content);

            Assert.Equal(content, this.requester.ConstructContent(requestInfo));
        }

        [Fact]
        public void SetsContentAsStreamContentIfBodyIsStream()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, new MemoryStream());
            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<StreamContent>(content);
        }

        [Fact]
        public void SetsContentAsStringContentIfBodyIsString()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, "hello");
            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<StringContent>(content);
        }

        [Fact]
        public void UsesRequestBodySerializerIfContentBodyIsObjectAndMethodIsSerialized()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            var body = new object();
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.Serialized, body);

            var requestBodySerializer = new Mock<RequestBodySerializer>();
            this.requester.RequestBodySerializer = requestBodySerializer.Object;

            var info = new RequestBodySerializerInfo(requestInfo);

            requestBodySerializer.Setup(x => x.SerializeBody(body, info)).Returns(new StringContent("test")).Verifiable();
            var content = this.requester.ConstructContent(requestInfo);

            requestBodySerializer.Verify();
            Assert.IsType<StringContent>(content);
        }

        [Fact]
        public void ThrowsIfBodyIsUrlEncodedAndBodyDoesNotImplementIDictionary()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            var body = new object();
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.UrlEncoded, body);

            Assert.Throws<ArgumentException>(() => this.requester.ConstructContent(requestInfo));
        }

        [Fact]
        public void UsesFormUrlEncodedSerializerIfBodyIsGenericDictionaryAndMethodIsUrlEncoded()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            // ExpandoObject implements IDictionary<string, object> but not IDictionary
            dynamic body = new ExpandoObject();
            body.foo = "bar woo";
            body.many = new List<string>() { "one", "two" };
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.UrlEncoded, body);

            var content = this.requester.ConstructContent(requestInfo);

            Assert.IsType<FormUrlEncodedContent>(content);
            var encodedContent = ((FormUrlEncodedContent)content).ReadAsStringAsync().Result;
            Assert.Equal("foo=bar+woo&many=one&many=two", encodedContent);
        }

        [Fact]
        public void UsesFormUrlEncodedSerializerIfBodyIsNonGenericDictionaryAndMethodIsUrlEncoded()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            IDictionary body = new Dictionary<string, object>()
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo((BodySerializationMethod)100, new object());

            Assert.Throws<InvalidOperationException>(() => this.requester.ConstructContent(requestInfo));
        }
    }
}
