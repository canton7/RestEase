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

        public interface IQueryParamWithUndecoratedParameter
        {
            [Get("foo")]
            Task FooAsync(string foo);
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
        public interface IHasNonOverriddenDefaultQuerySerializationMethodWithNoQueryAttribute
        {
            [Get("foo")]
            Task FooAsync(string foo);
        }

        [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
        public interface IHasOverriddenDefaultQuerySerializationMethod
        {
            [Get("foo")]
            Task FooAsync([Query(QuerySerializationMethod.ToString)] string foo);
        }

        public interface IHasNullQueryKey
        {
            [Get("foo")]
            Task FooAsync([Query(null)] string rawQuery);
        }

        public interface IHasEmptystringQueryKey
        {
            [Get("foo")]
            Task FooAsync([Query("")] string rawQuery);
        }

        public interface IHasFormat
        {
            [Get("foo")]
            Task FooAsync([Query(Format = "X")] int foo);
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
            Assert.Equal(1, requestInfo.QueryParams.Count());

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
            Assert.Equal("boo", requestInfo.Path);
        }

        [Fact]
        public void QueryParamWithImplicitNameCallsCorrectly()
        {
            var requestInfo = Request<IQueryParamWithImplicitName>(x => x.FooAsync("the value"));

            Assert.Equal(1, requestInfo.QueryParams.Count());

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void QueryParamWithUndecoratedParameterCallsCorrectly()
        {
            var requestInfo = Request<IQueryParamWithUndecoratedParameter>(x => x.FooAsync("the value"));

            Assert.Equal(1, requestInfo.QueryParams.Count());

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void HandlesMultipleQueryParamsWithTheSameName()
        {
            var requestInfo = Request<ITwoQueryParametersWithTheSameName>(x => x.FooAsync("foo value", "bar value"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(2, queryParams.Count);

            var queryParam0 = queryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("foo value", queryParam0.Value);

            var queryParam1 = queryParams[1].SerializeToString().First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("bar value", queryParam1.Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var requestInfo = Request<ISingleParameterWithQueryParamAttributeNoReturn>(x => x.BooAsync("yay"));

            Assert.Equal(1, requestInfo.QueryParams.Count());
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var requestInfo = Request<ISerializedQueryParam>(x => x.FooAsync("boom"));

            Assert.Equal(1, requestInfo.QueryParams.Count());
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var requestInfo = Request<IHasNonOverriddenDefaultQuerySerializationMethod>(x => x.FooAsync("boom"));

            Assert.Equal(1, requestInfo.QueryParams.Count());
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsHeaderWhenNoQueryAttributeIsPresent()
        {
            var requestInfo = Request<IHasNonOverriddenDefaultQuerySerializationMethodWithNoQueryAttribute>(x => x.FooAsync("boom"));

            Assert.Equal(1, requestInfo.QueryParams.Count());
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var requestInfo = Request<IHasOverriddenDefaultQuerySerializationMethod>(x => x.FooAsync("boom"));

            Assert.Equal(1, requestInfo.QueryParams.Count());
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void SerializesNullQueryKeys()
        {
            var requestInfo = Request<IHasNullQueryKey>(x => x.FooAsync("boom"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(1, queryParams.Count);
            Assert.Equal(null, queryParams[0].SerializeToString().First().Key);
        }

        [Fact]
        public void SerializesEmptystringQueryKeys()
        {
            var requestInfo = Request<IHasEmptystringQueryKey>(x => x.FooAsync("boom"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(1, queryParams.Count);
            Assert.Equal(String.Empty, queryParams[0].SerializeToString().First().Key);
        }

        [Fact]
        public void HandlesFormattedQueryParams()
        {
            var requestInfo = Request<IHasFormat>(x => x.FooAsync(11));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal("B", queryParams[0].SerializeToString().First().Value);
        }

        private IRequestInfo Request<T>(Func<T, Task> selector)
        {
            var implementation = this.builder.CreateImplementation<T>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            selector(implementation);

            return requestInfo;
        }
    }
}
