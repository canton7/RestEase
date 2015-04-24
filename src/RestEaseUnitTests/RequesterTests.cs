using Moq;
using RestEase;
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

            public new Uri ConstructUri(RequestInfo requestInfo)
            {
                return base.ConstructUri(requestInfo);
            }

            public new HttpContent ConstructContent(RequestInfo requestInfo)
            {
                return base.ConstructContent(requestInfo);
            }

            public new void ApplyHeaders(RequestInfo requestInfo, HttpRequestMessage requestMessage)
            {
                base.ApplyHeaders(requestInfo, requestMessage);
            }

            public new Task<HttpResponseMessage> SendRequestAsync(RequestInfo requestInfo)
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
            public RequestInfo RequestInfo;

            protected override Task<HttpResponseMessage> SendRequestAsync(RequestInfo requestInfo)
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

        [Fact]
        public void AppliesHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.AddClassHeader("User-Agent: RestEase");
            requestInfo.AddClassHeader("X-API-Key: Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.AddMethodHeader("User-Agent: RestEase");
            requestInfo.AddMethodHeader("X-API-Key: Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.AddHeaderParameter("User-Agent", "RestEase");
            requestInfo.AddHeaderParameter("X-API-Key", "Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void HeadersFromMethodOverrideHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.AddClassHeader("This-Will-Stay: YesIWill");
            requestInfo.AddClassHeader("Something: SomethingElse");
            requestInfo.AddClassHeader("User-Agent: RestEase");
            requestInfo.AddClassHeader("X-API-Key: Foo");

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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
            requestInfo.AddMethodHeader("Content-Type: text/html");

            var message = new HttpRequestMessage();
            Assert.Throws<ArgumentException>(() => this.requester.ApplyHeaders(requestInfo, message));
        }

        [Fact]
        public void RequestVoidAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            requester.ResponseMessage = Task.FromResult(new HttpResponseMessage());

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", cancellationToken);
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

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);
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

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", cancellationToken);
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

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);

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

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo", CancellationToken.None);

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
    }
}
