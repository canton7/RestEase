using Moq;
using RestEase;
using RestEase.Implementation;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class RequesterIntegrationTests : ImplementationFactoryTestsBase
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

        public interface INoArgumentsReturnsStream
        {
            [Get("bar")]
            Task<Stream> FooAsync();
        }

        public interface IAllRequestMethods
        {
            [Request("GET", "foo")]
            Task RequestAsync();

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

            [Patch("foo")]
            Task PatchAsync();
        }

        public interface IHasEmptyPath
        {
            [Get("")]
            Task FooAsync();
        }

        public RequesterIntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void NoArgumentsNoReturnCallsCorrectly()
        {
            var requestInfo = this.Request<INoArgumentsNoReturn>(x => x.FooAsync());

            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("foo", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithReturnCallsCorrectly()
        {
            var requestInfo = this.Request<INoArgumentsWithReturn, int>(x => x.BarAsync(), 3);

            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithResponseCallsCorrectly()
        {
            var response = new Response<string>("hello", new HttpResponseMessage(), () => null);
            var requestInfo = this.RequestWithResponse<INoArgumentsReturnsResponse, string>(x => x.FooAsync(), response);

            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithResponseMessageCallsCorrectly()
        {
            var response = new HttpResponseMessage();
            var requestInfo = this.RequestWithResponseMessage<INoArgumentsReturnsHttpResponseMessage>(x => x.FooAsync(), response);
            
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithRawResponseCallsCorrectly()
        {
            var requestInfo = this.RequestRaw<INoArgumentsReturnsString>(x => x.FooAsync(), "testy");

            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void NoArgumentsWithStreamResponseCallsCorrectly()
        {
            var requestInfo = this.RequestStream<INoArgumentsReturnsStream>(x => x.FooAsync(), new MemoryStream());

            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("bar", requestInfo.Path);
        }

        [Fact]
        public void AllHttpMethodsSupported()
        {
            var implementation = this.CreateImplementation<IAllRequestMethods>();
            IRequestInfo requestInfo = null;

            this.Requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.RequestAsync();
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);

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

            implementation.TraceAsync();
            Assert.Equal(new HttpMethod("TRACE"), requestInfo.Method);
            Assert.Equal("foo", requestInfo.Path);
        }

        [Fact]
        public void AllowsEmptyPath()
        {
            var requestInfo = this.Request<IHasEmptyPath>(x => x.FooAsync());

            Assert.Equal("", requestInfo.Path);
        }
    }
}
