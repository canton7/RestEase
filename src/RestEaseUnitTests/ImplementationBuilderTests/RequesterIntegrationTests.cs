using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class RequesterIntegrationTests
    {
        public interface INoArgumentsNoReturn
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface INoArgumentsWithReturn
        {
            [Get("bar")]
            Task<int> BarAsync();
        }

        public interface INoArgumentsReturnsResponse
        {
            [Get("bar")]
            Task<Response<string>> FooAsync();
        }

        public interface INoArgumentsReturnsHttpResponseMessage
        {
            [Get("bar")]
            Task<HttpResponseMessage> FooAsync();
        }

        public interface INoArgumentsReturnsString
        {
            [Get("bar")]
            Task<string> FooAsync();
        }

        public interface IAllRequestMethods
        {
            [Delete("foo")]
            Task DeleteAsync();

            [Get("foo")]
            Task GetAsync();

            [Head("foo")]
            Task HeadAsync();

            [Options("foo")]
            Task OptionsAsync();

            [Post("foo")]
            Task PostAsync();

            [Put("foo")]
            Task PutAsync();

            [Trace("foo")]
            Task TraceAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void NoArgumentsNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsNoReturn>(this.requester.Object);

            var expectedResponse = Task.FromResult(false);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.FooAsync();

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("foo", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsWithReturn>(this.requester.Object);

            var expectedResponse = Task.FromResult(3);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestAsync<int>(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.BarAsync();

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithResponseCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsReturnsResponse>(this.requester.Object);

            var expectedResponse = Task.FromResult(new Response<string>("hello", new HttpResponseMessage(), () => null));
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestWithResponseAsync<string>(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.FooAsync();

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithResponseMessageCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsReturnsHttpResponseMessage>(this.requester.Object);

            var expectedResponse = Task.FromResult(new HttpResponseMessage());
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestWithResponseMessageAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.FooAsync();

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithRawResponseCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsReturnsString>(this.requester.Object);

            var expectedResponse = Task.FromResult("testy");
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestRawAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.FooAsync();

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void AllHttpMethodsSupported()
        {
            var implementation = this.builder.CreateImplementation<IAllRequestMethods>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.DeleteAsync();
            Assert.Equal(HttpMethod.Delete, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.GetAsync();
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.HeadAsync();
            Assert.Equal(HttpMethod.Head, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.OptionsAsync();
            Assert.Equal(HttpMethod.Options, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.PostAsync();
            Assert.Equal(HttpMethod.Post, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.PutAsync();
            Assert.Equal(HttpMethod.Put, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

            implementation.TraceAsync();
            Assert.Equal(HttpMethod.Trace, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);
        }
    }
}
