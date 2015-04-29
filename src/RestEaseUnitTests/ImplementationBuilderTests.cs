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

namespace RestEaseUnitTests
{
    public class ImplementationBuilderTests
    {
        public interface IMethodWithoutAttribute
        {
            [Get("foo")]
            Task GetAsync();

            Task SomethingElseAsync();
        }

        public interface IMethodReturningVoid
        {
            [Get("foo")]
            Task GetAsync();

            void ReturnsVoid();
        }

        public interface IMethodReturningString
        {
            [Get("foo")]
            Task GetAsync();

            string ReturnsString();
        }

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

        public interface IQueryParamWithImplicitName
        {
            [Get("foo")]
            Task FooAsync([QueryParam] string foo);
        }

        public interface ITwoQueryParametersWithTheSameName
        {
            [Get("foo")]
            Task FooAsync([QueryParam("bar")] string foo, [QueryParam] string bar);
        }

        public interface INullableQueryParameters
        {
            [Get("foo")]
            Task FooAsync(object foo, int? bar, int? baz, int yay);
        }

        public interface IArrayQueryParam
        {
            [Get("foo")]
            Task FooAsync(IEnumerable<int> intArray);
        }

        public interface IPathParams
        {
            [Get("foo/{foo}/{bar}")]
            Task FooAsync([PathParam] string foo, [PathParam("bar")] string bar);

            [Get("foo/{foo}/{bar}")]
            Task DifferentParaneterTypesAsync([PathParam] object foo, [PathParam] int? bar);
        }

        [Header("Class Header 1")]
        [Header("Class Header 2")]
        public interface IHasClassHeaders
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasMethodHeaders
        {
            [Get("foo")]
            [Header("Method Header 1")]
            [Header("Method Header 2")]
            Task FooAsync();
        }

        public interface IHasParamHeaders
        {
            [Get("foo")]
            Task FooAsync([Header("Param Header 1")] string foo, [Header("Param Header 2")] string bar);
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

        public interface IHasTwoBodies
        {
            [Get("foo")]
            Task FooAsync([Body] string body1, [Body] string body2);
        }

        public interface IHasBody
        {
            [Get("foo")]
            Task SerializedAsync([Body(BodySerializationMethod.Serialized)] object serialized);

            [Get("bar")]
            Task UrlEncodedAsync([Body(BodySerializationMethod.UrlEncoded)] object serialized);

            [Get("bar")]
            Task ValueTypeAsync([Body(BodySerializationMethod.UrlEncoded)] int serialized);
        }

        public interface IHasPathParamInPathButNotParameters
        {
            [Get("foo/{bar}/{baz}")]
            Task FooAsync([PathParam("bar")] string bar);
        }

        public interface IHasPathParamInParametersButNotPath
        {
            [Get("foo/{bar}")]
            Task FooAsync([PathParam("bar")] string path, [PathParam("baz")] string baz);
        }

        public interface IHasPathParamWithoutExplicitName
        {
            [Get("foo/{bar}")]
            Task FooAsync([PathParam] string bar);
        }

        private readonly Mock<IRequester> requester;
        private readonly ImplementationBuilder builder;

        public ImplementationBuilderTests()
        {
            this.requester = new Mock<IRequester>(MockBehavior.Strict);
            this.builder = new ImplementationBuilder();
        }

        [Fact]
        public void ThrowsIfMethodWithoutAttribute()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IMethodWithoutAttribute>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningVoid()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IMethodReturningVoid>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningString()
        {
            // Ideally we would test every object that isn't a Task<T>, but that's somewhat impossible...
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IMethodReturningString>(this.requester.Object));
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

            var expectedResponse = Task.FromResult(3);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestAsync<int>(It.IsAny<RequestInfo>()))
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
        public void NoArgumentsWithResponseCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<INoArgumentsReturnsResponse>(this.requester.Object);

            var expectedResponse = Task.FromResult(new Response<string>(new HttpResponseMessage(), "hello"));
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestWithResponseAsync<string>(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestWithResponseMessageAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestRawAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<ITwoCancellationTokens>(this.requester.Object));
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

        [Fact]
        public void QueryParamWithImplicitNameCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<IQueryParamWithImplicitName>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("the value");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal("foo", requestInfo.QueryParams[0].Key);
            Assert.Equal("the value", requestInfo.QueryParams[0].Value);
        }

        [Fact]
        public void HandlesMultipleQueryParamsWithTheSameName()
        {
            var implementation = this.builder.CreateImplementation<ITwoQueryParametersWithTheSameName>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("foo value", "bar value");

            Assert.Equal(2, requestInfo.QueryParams.Count);

            Assert.Equal("bar", requestInfo.QueryParams[0].Key);
            Assert.Equal("foo value", requestInfo.QueryParams[0].Value);

            Assert.Equal("bar", requestInfo.QueryParams[1].Key);
            Assert.Equal("bar value", requestInfo.QueryParams[1].Value);
        }

        [Fact]
        public void ExcludesNullQueryParams()
        {
            var implementation = this.builder.CreateImplementation<INullableQueryParameters>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(null, null, 0, 0);

            Assert.Equal(4, requestInfo.QueryParams.Count);

            Assert.Equal("foo", requestInfo.QueryParams[0].Key);
            Assert.Equal(null, requestInfo.QueryParams[0].Value);

            Assert.Equal("bar", requestInfo.QueryParams[1].Key);
            Assert.Equal(null, requestInfo.QueryParams[1].Value);

            Assert.Equal("baz", requestInfo.QueryParams[2].Key);
            Assert.Equal("0", requestInfo.QueryParams[2].Value);

            Assert.Equal("yay", requestInfo.QueryParams[3].Key);
            Assert.Equal("0", requestInfo.QueryParams[3].Value);
        }

        [Fact]
        public void HandlesQueryParamArays()
        {
            var implementation = this.builder.CreateImplementation<IArrayQueryParam>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new int[] { 1, 2, 3 });

            Assert.Equal(3, requestInfo.QueryParams.Count);

            Assert.Equal("intArray", requestInfo.QueryParams[0].Key);
            Assert.Equal("1", requestInfo.QueryParams[0].Value);

            Assert.Equal("intArray", requestInfo.QueryParams[1].Key);
            Assert.Equal("2", requestInfo.QueryParams[1].Value);

            Assert.Equal("intArray", requestInfo.QueryParams[2].Key);
            Assert.Equal("3", requestInfo.QueryParams[2].Value);
        }

        [Fact]
        public void HandlesNullPathParams()
        {
            var implementation = this.builder.CreateImplementation<IPathParams>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.DifferentParaneterTypesAsync(null, null);

            Assert.Equal(2, requestInfo.PathParams.Count);

            Assert.Equal("foo", requestInfo.PathParams[0].Key);
            Assert.Equal(null, requestInfo.PathParams[0].Value);

            Assert.Equal("bar", requestInfo.PathParams[1].Key);
            Assert.Equal(null, requestInfo.PathParams[1].Value);
        }

        [Fact]
        public void NullPathParamsAreRenderedAsEmpty()
        {
            var implementation = this.builder.CreateImplementation<IPathParams>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("foo value", "bar value");

            Assert.Equal(2, requestInfo.PathParams.Count);

            Assert.Equal("foo", requestInfo.PathParams[0].Key);
            Assert.Equal("foo value", requestInfo.PathParams[0].Value);

            Assert.Equal("bar", requestInfo.PathParams[1].Key);
            Assert.Equal("bar value", requestInfo.PathParams[1].Value);
        }

        [Fact]
        public void HandlesClassHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasClassHeaders>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Equal(new[] { "Class Header 1", "Class Header 2" }, requestInfo.ClassHeaders.OrderBy(x => x));
        }

        [Fact]
        public void HandlesMethodHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasMethodHeaders>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Equal(new[] { "Method Header 1", "Method Header 2" }, requestInfo.MethodHeaders.OrderBy(x => x));
        }

        [Fact]
        public void HandlesParamHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasParamHeaders>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("value 1", "value 2");

            Assert.Equal(2, requestInfo.HeaderParams.Count);

            Assert.Equal("Param Header 1", requestInfo.HeaderParams[0].Key);
            Assert.Equal("value 1", requestInfo.HeaderParams[0].Value);

            Assert.Equal("Param Header 2", requestInfo.HeaderParams[1].Key);
            Assert.Equal("value 2", requestInfo.HeaderParams[1].Value);
        }

        [Fact]
        public void AllHttpMethodsSupported()
        {
            var implementation = this.builder.CreateImplementation<IAllRequestMethods>(this.requester.Object);
            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
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

        [Fact]
        public void ThrowsIfTwoBodies()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasTwoBodies>(this.requester.Object));
        }

        [Fact]
        public void BodyWithSerializedClassAsExpected()
        {
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var body = new object();
            implementation.SerializedAsync(body);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.Value);
        }

        [Fact]
        public void BodyWithUrlEncodedCallsAsExpected()
        {
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var body = new object();
            implementation.UrlEncodedAsync(body);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.Value);
        }

        [Fact]
        public void BodyWithValueTypeCallsAsExpected()
        {
            // Tests that the value is boxed properly
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            RequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<RequestInfo>()))
                .Callback((RequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.ValueTypeAsync(3);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(3, requestInfo.BodyParameterInfo.Value);
        }

        [Fact]
        public void ThrowsIfPathParamPresentInPathButNotInParameters()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPathParamInPathButNotParameters>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPathParamPresentInParametersButNotPath()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPathParamInParametersButNotPath>(this.requester.Object));
        }

        [Fact]
        public void PathParamWithImplicitNameDoesNotFailValidation()
        {
            this.builder.CreateImplementation<IHasPathParamWithoutExplicitName>(this.requester.Object);
        }
    }
}
