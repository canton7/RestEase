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
    public class QueryParamTests
    {
        public interface ISingleParameterWithQueryParamAttributeNoReturn
        {
            [Get("boo")]
            Task BooAsync([Query("bar")] string foo);
        }

        public interface IQueryParamWithImplicitName
        {
            [Get("foo")]
            Task FooAsync([Query] string foo);
        }

        public interface ITwoQueryParametersWithTheSameName
        {
            [Get("foo")]
            Task FooAsync([Query("bar")] string foo, [Query] string bar);
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

        public interface ISerializedQueryParam
        {
            [Get("foo")]
            Task FooAsync([Query(QuerySerializationMethod.Serialized)] object foo);
        }

        [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
        public interface IHasNonOverriddenDefaultQuerySerializationMethod
        {
            [Get("foo")]
            Task FooAsync([Query] string foo);
        }

        [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
        public interface IHasOverriddenDefaultQuerySerializationMethod
        {
            [Get("foo")]
            Task FooAsync([Query(QuerySerializationMethod.ToString)] string foo);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void SingleParameterWithQueryParamAttributeNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<ISingleParameterWithQueryParamAttributeNoReturn>(this.requester.Object);

            var expectedResponse = Task.FromResult(false);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.BooAsync("the value");

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(1, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
            Assert.Equal("boo", requestInfo.Path);
        }

        [Fact]
        public void QueryParamWithImplicitNameCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<IQueryParamWithImplicitName>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("the value");

            Assert.Equal(1, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void HandlesMultipleQueryParamsWithTheSameName()
        {
            var implementation = this.builder.CreateImplementation<ITwoQueryParametersWithTheSameName>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("foo value", "bar value");

            Assert.Equal(2, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("foo value", queryParam0.Value);

            var queryParam1 = requestInfo.QueryParams[1].SerializeToString().First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("bar value", queryParam1.Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<ISingleParameterWithQueryParamAttributeNoReturn>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.BooAsync("yay");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<ISerializedQueryParam>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("boom");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var implementation = this.builder.CreateImplementation<IHasNonOverriddenDefaultQuerySerializationMethod>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("boom");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var implementation = this.builder.CreateImplementation<IHasOverriddenDefaultQuerySerializationMethod>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("boom");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams[0].SerializationMethod);
        }
    }
}
