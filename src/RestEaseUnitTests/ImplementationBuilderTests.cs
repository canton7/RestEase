using Moq;
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
    public class ImplementationBuilderTests
    {
        public interface INoArgumentsNoReturn
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface INoArgumentsWithReturn
        {
            [Get("bar")]
            Task<string> BarAsync();
        }

        public interface ICancellationTokenOnlyNoReturn
        {
            [Get("baz")]
            Task BazAsync(CancellationToken cancellationToken);
        }

        public interface ITwoCancellationTokens
        {
            [Get("yay")]
            Task YayAsync(CancellationToken cancellationToken1, CancellationToken cancellationToken2);
        }

        public interface ISingleParameterWithQueryParamAttributeNoReturn
        {
            [Get("boo")]
            Task BooAsync([QueryParam("bar")] string foo);
        }

        private readonly Mock<IRequester> requester;
        private readonly ImplementationBuilder builder;

        public ImplementationBuilderTests()
        {
            this.requester = new Mock<IRequester>(MockBehavior.Strict);
            this.builder = new ImplementationBuilder();
        }

        [Fact]
        public void NoArgumentsNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsNoReturn>(this.requester.Object);
            
            var expectedResponse = Task.FromResult(false);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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

            var expectedResponse = Task.FromResult("hello");
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestAsync<string>(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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
        public void CancellationTokenOnlyNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<ICancellationTokenOnlyNoReturn>(this.requester.Object);
            
            var expectedResponse = Task.FromResult(false);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var cts = new CancellationTokenSource();
            var response = implementation.BazAsync(cts.Token);

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(cts.Token, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(0, requestInfo.QueryParams.Count);
            Assert.Equal("baz", requestInfo.Path);
        }

        [Fact]
        public void TwoCancellationTokensThrows()
        {
            Assert.Throws<RestEaseImplementationCreationException>(() => this.builder.CreateImplementation<ITwoCancellationTokens>(this.requester.Object));
        }

        [Fact]
        public void SingleParameterWithQueryParamAttributeNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<ISingleParameterWithQueryParamAttributeNoReturn>(this.requester.Object);

            var expectedResponse = Task.FromResult(false);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.BooAsync("the value");

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal("bar", requestInfo.QueryParams[0].Key);
            Assert.Equal("the value", requestInfo.QueryParams[0].Value);
            Assert.Equal("boo", requestInfo.Path);
        }
    }
}
