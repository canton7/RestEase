using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            public new Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo)
            {
                return base.SendRequestAsync(requestInfo);
            }
        }

        private class RequesterWithStubbedSendRequestAsync : Requester
        {
            public RequesterWithStubbedSendRequestAsync(HttpClient httpClient)
                : base(httpClient)
            { }

            public Task<HttpResponseMessage> ResponseMessage;
            public IRequestInfo RequestInfo;

            protected override Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo)
            {
                this.RequestInfo = requestInfo;
                return this.ResponseMessage;
            }
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage Request;
            public Task<HttpResponseMessage> ResponseMessage;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Can't test against the CancellationToken, as HttpClient does some odd stuff to it
                this.Request = request;
                return this.ResponseMessage;
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
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("", requestInfo);
            Assert.Equal(new Uri("/", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathNoLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("foo/bar/baz", requestInfo);
            Assert.Equal(new Uri("/foo/bar/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithSimpleRelativePathWithLeadingSlash()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo/bar/baz", requestInfo);
            Assert.Equal(new Uri("/foo/bar/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedPath()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo/ba  r/baz", requestInfo);
            Assert.Equal(new Uri("/foo/ba%20%20r/baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriCombiningExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            var uri = this.requester.ConstructUri("/foo?a=yay", requestInfo);
            Assert.Equal(new Uri("/foo?a=yay&bar=baz", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=b az", requestInfo);
            Assert.Equal(new Uri("/foo?bar=b+az", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithEscapedGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("b ar", "b az");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?b+ar=b+az", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var uri = this.requester.ConstructUri("/foo?bar=baz&bar=baz2", requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz");
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }

        [Fact]
        public void ConstructsUriWithPreservedDuplicateExistingAndGivenParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter("bar", "baz2");
            var uri = this.requester.ConstructUri("/foo?bar=baz", requestInfo);
            Assert.Equal(new Uri("/foo?bar=baz&bar=baz2", UriKind.Relative), uri);
        }

        [Fact]
        public void IgnoresNullQueryParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter<object>("bar", null);
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo", UriKind.Relative), uri);
        }

        [Fact]
        public void AddsParamsFromQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "baz", "yay" },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?foo=bar&baz=yay", UriKind.Relative), uri);
        }

        [Fact]
        public void IgnoresNullItemsFromQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "baz", null },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?foo=bar", UriKind.Relative), uri);
        }

        [Fact]
        public void HandlesArraysInQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = new Dictionary<string, object>()
            {
                { "foo", new[] { "bar", "baz" } },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("/foo?foo=bar&foo=baz", UriKind.Relative), uri);
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
        public void UsesBodySerializerIfContentBodyIsObjectAndMethodIsSerialized()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            var body = new object();
            requestInfo.SetBodyParameterInfo(BodySerializationMethod.UrlEncoded, body);

            Assert.Throws<ArgumentException>(() => this.requester.ConstructContent(requestInfo));
        }

        [Fact]
        public void UsesFormUrlEncodedSerializerIfBodyIsObjectAndMethodIsUrlEncoded()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo((BodySerializationMethod)100, new object());

            Assert.Throws<InvalidOperationException>(() => this.requester.ConstructContent(requestInfo));
        }

        [Fact]
        public void AppliesHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.ClassHeaders = new List<string>() { "User-Agent: RestEase", "X-API-Key: Foo" };

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("User-Agent: RestEase");
            requestInfo.AddMethodHeader("X-API-Key: Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddHeaderParameter("User-Agent", "RestEase");
            requestInfo.AddHeaderParameter("X-API-Key", "Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void HeadersFromMethodOverrideHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.ClassHeaders = new List<string>()
            {
                "This-Will-Stay: YesIWill",
                "Something: SomethingElse",
                "User-Agent: RestEase",
                "X-API-Key: Foo",
            };

            requestInfo.AddMethodHeader("Something"); // Remove
            requestInfo.AddMethodHeader("User-Agent:"); // Replace with null
            requestInfo.AddMethodHeader("X-API-Key: Bar"); // Change value
            requestInfo.AddMethodHeader("This-Is-New: YesIAM"); // New value

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("This-Will-Stay: YesIWill\r\nThis-Is-New: YesIAM\r\nUser-Agent: \r\nX-API-Key: Bar\r\n", message.Headers.ToString());
        }

        [Fact]
        public void HeadersFromParamsOVerrideHeadersFromMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("This-Will-Stay: YesIWill");
            requestInfo.AddMethodHeader("Something: SomethingElse");
            requestInfo.AddMethodHeader("User-Agent: RestEase");
            requestInfo.AddMethodHeader("X-API-Key: Foo");

            requestInfo.AddHeaderParameter("Something", null); // Remove
            requestInfo.AddHeaderParameter("User-Agent", ""); // Replace with null
            requestInfo.AddHeaderParameter("X-API-Key", "Bar"); // Change value
            requestInfo.AddHeaderParameter("This-Is-New", "YesIAM"); // New value

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("This-Will-Stay: YesIWill\r\nThis-Is-New: YesIAM\r\nUser-Agent: \r\nX-API-Key: Bar\r\n", message.Headers.ToString());
        }

        [Fact]
        public void MultipleHeadersAreAllowed()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("User-Agent: SomethingElse");
            requestInfo.AddMethodHeader("User-Agent: RestEase");
            requestInfo.AddMethodHeader("X-API-Key: Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: SomethingElse RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void SingleOverrideReplacesMultipleHeaders()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("User-Agent: SomethingElse");
            requestInfo.AddMethodHeader("User-Agent: RestEase");
            requestInfo.AddMethodHeader("X-API-Key: Foo");

            requestInfo.AddHeaderParameter("User-Agent", null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("X-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesContentHeadersToContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("Content-Type: text/html");

            var message = new HttpRequestMessage();
            message.Content = new StringContent("hello");
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("", message.Headers.ToString());
            Assert.Equal("Content-Type: text/html\r\n", message.Content.Headers.ToString());
        }

        [Fact]
        public void DoesNotAttemptToApplyContentHeadersIfThereIsNoContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("Content-Type: text/html");

            var message = new HttpRequestMessage();
            Assert.Throws<ArgumentException>(() => this.requester.ApplyHeaders(requestInfo, message));
        }

        [Fact]
        public void RequestVoidAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            requester.ResponseMessage = Task.FromResult(new HttpResponseMessage());

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requester.RequestVoidAsync(requestInfo).Wait();

            Assert.Equal(requestInfo, requester.RequestInfo);
        }

        [Fact]
        public void RequestAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            var responseMessage = new HttpResponseMessage();
            requester.ResponseMessage = Task.FromResult(responseMessage);
            var responseDeserializer = new Mock<IResponseDeserializer>();
            requester.ResponseDeserializer = responseDeserializer.Object;
            var cancellationToken = new CancellationToken();

            responseDeserializer.Setup(x => x.ReadAndDeserializeAsync<string>(responseMessage, cancellationToken))
                .Returns(Task.FromResult("hello"))
                .Verifiable();

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.CancellationToken = cancellationToken;
            var result = requester.RequestAsync<string>(requestInfo).Result;

            responseDeserializer.Verify();

            Assert.Equal(requestInfo, requester.RequestInfo);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void RequestWithResponseMessageAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            var responseMessage = new HttpResponseMessage();
            requester.ResponseMessage = Task.FromResult(responseMessage);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            var result = requester.RequestWithResponseMessageAsync(requestInfo).Result;

            Assert.Equal(requestInfo, requester.RequestInfo);
            Assert.Equal(responseMessage, result);
        }

        [Fact]
        public void RequestWithResponseAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            var responseMessage = new HttpResponseMessage();
            requester.ResponseMessage = Task.FromResult(responseMessage);
            var responseDeserializer = new Mock<IResponseDeserializer>();
            requester.ResponseDeserializer = responseDeserializer.Object;
            var cancellationToken = new CancellationToken();

            responseDeserializer.Setup(x => x.ReadAndDeserializeAsync<string>(responseMessage, cancellationToken))
                .Returns(Task.FromResult("hello"))
                .Verifiable();

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.CancellationToken = cancellationToken;
            var result = requester.RequestWithResponseAsync<string>(requestInfo).Result;

            responseDeserializer.Verify();

            Assert.Equal(requestInfo, requester.RequestInfo);
            Assert.Equal("hello", result.Content);
            Assert.Equal(responseMessage, result.ResponseMessage);
        }

        [Fact]
        public void SendRequestAsyncSendsRequest()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new MyRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");

            var responseMessage = new HttpResponseMessage();
            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var response = requester.SendRequestAsync(requestInfo).Result;

            Assert.Equal("http://api.com/foo", messageHandler.Request.RequestUri.ToString());
            Assert.Equal(responseMessage, response);
        }

        [Fact]
        public void SendRequestThrowsIfResponseIsBad()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new MyRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");

            var responseMessage = new HttpResponseMessage();
            responseMessage.Headers.Add("Foo", "bar");
            responseMessage.StatusCode = HttpStatusCode.NotFound;
            responseMessage.Content = new StringContent("hello");

            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var aggregateException = Assert.Throws<AggregateException>(() => requester.SendRequestAsync(requestInfo).Wait());
            var e = Assert.IsType<ApiException>(aggregateException.InnerException);

            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
            Assert.True(e.Headers.Contains("Foo"));
            Assert.True(e.HasContent);
            Assert.Equal("hello", e.Content);
        }

        [Fact]
        public void SendRequestDoesNotThrowIfResponseIsBadButAllowAnyStatusCodeSpecified()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new MyRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AllowAnyStatusCode = true;

            var responseMessage = new HttpResponseMessage();
            responseMessage.StatusCode = HttpStatusCode.NotFound;

            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var response = requester.SendRequestAsync(requestInfo).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
