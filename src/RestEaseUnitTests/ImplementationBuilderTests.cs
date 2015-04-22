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
            Assert.Equal(0, requestInfo.Parameters.Count);
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
            Assert.Equal(0, requestInfo.Parameters.Count);
            Assert.Equal("bar", requestInfo.Path);
        }
    }
}
