using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestEase.UnitTests.RequesterTests
{
    public class SendRequestTests
    {
        private class RequesterWithStubbedSendRequestAsync : Requester
        {
            public RequesterWithStubbedSendRequestAsync(HttpClient httpClient)
                : base(httpClient)
            { }

            public Task<HttpResponseMessage> ResponseMessage;
            public IRequestInfo RequestInfo;

            protected override Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo, bool readBody)
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

        [Fact]
        public void RequestVoidAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null)
            {
                ResponseMessage = Task.FromResult(new HttpResponseMessage())
            };
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requester.RequestVoidAsync(requestInfo).Wait();

            Assert.Equal(requestInfo, requester.RequestInfo);
        }

        [Fact]
        public void RequestAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            var responseMessage = new HttpResponseMessage()
            {
                Content = new StringContent("content"),
            };
            requester.ResponseMessage = Task.FromResult(responseMessage);
            var responseDeserializer = new Mock<ResponseDeserializer>();
            requester.ResponseDeserializer = responseDeserializer.Object;
            var cancellationToken = new CancellationToken();

            responseDeserializer.Setup(x => x.Deserialize<string>("content", responseMessage, It.IsAny<ResponseDeserializerInfo>()))
                .Returns("hello")
                .Verifiable();

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                CancellationToken = cancellationToken
            };
            string result = requester.RequestAsync<string>(requestInfo).Result;

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
        public void RequestInfoHttpMessagePropertiesAddedToRequestMessage()
        {
            var messageHandler = new MockHttpMessageHandler { ResponseMessage = Task.FromResult(new HttpResponseMessage()) };
            var httpClient = new HttpClient(messageHandler);
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddHttpRequestMessagePropertyProperty("key1", "value1");
            requestInfo.AddHttpRequestMessagePropertyParameter("key2", "value2");
            requester.RequestWithResponseMessageAsync(requestInfo).Wait();

#if NET452 || NETCOREAPP1_0 || NETCOREAPP2_0 || NETCOREAPP3_0
            Assert.Equal(3, messageHandler.Request.Properties.Count);
            Assert.Equal("value1", messageHandler.Request.Properties["key1"]);
            Assert.Equal("value2", messageHandler.Request.Properties["key2"]);
#else
            Assert.True(messageHandler.Request.Options.TryGetValue(new HttpRequestOptionsKey<string>("key1"), out string key1));
            Assert.Equal("value1", key1);
            Assert.True(messageHandler.Request.Options.TryGetValue(new HttpRequestOptionsKey<string>("key2"), out string key2));
            Assert.Equal("value2", key2);
#endif
        }

        [Fact]
        public void RequestWithResponseAsyncSendsRequest()
        {
            var requester = new RequesterWithStubbedSendRequestAsync(null);
            var responseMessage = new HttpResponseMessage()
            {
                Content = new StringContent("content"),
            };
            requester.ResponseMessage = Task.FromResult(responseMessage);
            var responseDeserializer = new Mock<ResponseDeserializer>();
            requester.ResponseDeserializer = responseDeserializer.Object;
            var cancellationToken = new CancellationToken();

            responseDeserializer.Setup(x => x.Deserialize<string>("content", responseMessage, It.IsAny<ResponseDeserializerInfo>()))
                .Returns("hello")
                .Verifiable();

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                CancellationToken = cancellationToken
            };
            var result = requester.RequestWithResponseAsync<string>(requestInfo).Result;

            string deserializedContent = result.GetContent();

            responseDeserializer.Verify();

            Assert.Equal(requestInfo, requester.RequestInfo);
            Assert.Equal("content", result.StringContent);
            Assert.Equal("hello", deserializedContent);
            Assert.Equal(responseMessage, result.ResponseMessage);
        }

        [Fact]
        public void SendRequestAsyncSendsRequest()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");

            var responseMessage = new HttpResponseMessage();
            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var response = requester.SendRequestAsync(requestInfo, true).Result;

            Assert.Equal("http://api.com/foo", messageHandler.Request.RequestUri.ToString());
            Assert.Equal(responseMessage, response);
        }

        [Fact]
        public void SendRequestThrowsIfResponseIsBad()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new PublicRequester(httpClient);
            var responseDeserializer = new Mock<ResponseDeserializer>();
            requester.ResponseDeserializer = responseDeserializer.Object;

            var requestInfo = new RequestInfo(HttpMethod.Post, "foo");

            var responseMessage = new HttpResponseMessage();
            responseMessage.Headers.Add("Foo", "bar");
            responseMessage.StatusCode = HttpStatusCode.NotFound;
            responseMessage.Content = new StringContent("hello");

            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var aggregateException = Assert.Throws<AggregateException>(() => requester.SendRequestAsync(requestInfo, true).Wait());
            var e = Assert.IsType<ApiException>(aggregateException.InnerException);

            Assert.Equal(HttpMethod.Post, e.RequestMethod);
            Assert.Equal("http://api.com/foo", e.RequestUri.ToString());
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
            Assert.True(e.Headers.Contains("Foo"));
            Assert.True(e.HasContent);
            Assert.Equal("hello", e.Content);

            responseDeserializer.Setup(x => x.Deserialize<int>(
                "hello",
                responseMessage,
                It.Is<ResponseDeserializerInfo>(x => x.RequestInfo == requestInfo)))
                .Returns(10);
            Assert.Equal(10, e.DeserializeContent<int>());
        }

        [Fact]
        public void SendRequestDoesNotThrowIfResponseIsBadButAllowAnyStatusCodeSpecified()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.com") };
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                AllowAnyStatusCode = true
            };
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            messageHandler.ResponseMessage = Task.FromResult(responseMessage);

            var response = requester.SendRequestAsync(requestInfo, true).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void AllowsNullPath()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.example.com/base/") };
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, null);

            messageHandler.ResponseMessage = Task.FromResult(new HttpResponseMessage());
            requester.SendRequestAsync(requestInfo, true).Wait();
            Assert.Equal("http://api.example.com/base/", messageHandler.Request.RequestUri.ToString());
        }

        [Fact]
        public void AllowsEmptyPath()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.example.com/base") };
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "");

            messageHandler.ResponseMessage = Task.FromResult(new HttpResponseMessage());

            requester.SendRequestAsync(requestInfo, true).Wait();
            Assert.Equal("http://api.example.com/base", messageHandler.Request.RequestUri.ToString());
        }

        [Fact]
        public void AllowsPathWithLeadingSlash()
        {
            var messageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://api.example.com/base/") };
            var requester = new PublicRequester(httpClient);

            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo");

            messageHandler.ResponseMessage = Task.FromResult(new HttpResponseMessage());

            requester.SendRequestAsync(requestInfo, true).Wait();
            Assert.Equal("http://api.example.com/base/foo", messageHandler.Request.RequestUri.ToString());
        }
    }
}
