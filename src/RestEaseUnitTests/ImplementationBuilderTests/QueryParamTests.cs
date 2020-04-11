using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
            Task FooAsync(object foo, string bar, int? baz);
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
            Task FooAsync([Query(Format = "C")] int foo);
        }

        public interface IHasPathQuery
        {
            [Query("foo")]
            string Foo { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasFormattedPathQuery
        {
            [Query(Format = "X")]
            int Woo { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasQueryParameterNameUsingAttributeSetter
        {
            [Get]
            Task FooAsync([Query(Name = "customName")] string queryParameter);
        }

        public interface IHasQueryParameterNameUsingAttributeConstructor
        {
            [Get]
            Task FooAsync([Query("customName")] string queryParameter);
        }

        public interface IHasSerializedPathQuery
        {
            [Query(SerializationMethod = QuerySerializationMethod.Serialized)]
            string Yay { get; set; }

            [Get]
            Task FooAsync();
        }


        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

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
            Assert.Single(requestInfo.QueryParams);

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString(null).First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
            Assert.Equal("boo", requestInfo.Path);
        }

        [Fact]
        public void QueryParamWithImplicitNameCallsCorrectly()
        {
            var requestInfo = Request<IQueryParamWithImplicitName>(x => x.FooAsync("the value"));

            Assert.Single(requestInfo.QueryParams);

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString(null).First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void QueryParamWithUndecoratedParameterCallsCorrectly()
        {
            var requestInfo = Request<IQueryParamWithUndecoratedParameter>(x => x.FooAsync("the value"));

            Assert.Single(requestInfo.QueryParams);

            var queryParam0 = requestInfo.QueryParams.First().SerializeToString(null).First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void HandlesMultipleQueryParamsWithTheSameName()
        {
            var requestInfo = Request<ITwoQueryParametersWithTheSameName>(x => x.FooAsync("foo value", "bar value"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(2, queryParams.Count);

            var queryParam0 = queryParams[0].SerializeToString(null).First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("foo value", queryParam0.Value);

            var queryParam1 = queryParams[1].SerializeToString(null).First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("bar value", queryParam1.Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var requestInfo = Request<ISingleParameterWithQueryParamAttributeNoReturn>(x => x.BooAsync("yay"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var requestInfo = Request<ISerializedQueryParam>(x => x.FooAsync("boom"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var requestInfo = Request<IHasNonOverriddenDefaultQuerySerializationMethod>(x => x.FooAsync("boom"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsHeaderWhenNoQueryAttributeIsPresent()
        {
            var requestInfo = Request<IHasNonOverriddenDefaultQuerySerializationMethodWithNoQueryAttribute>(x => x.FooAsync("boom"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var requestInfo = Request<IHasOverriddenDefaultQuerySerializationMethod>(x => x.FooAsync("boom"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void SerializesNullQueryKeys()
        {
            var requestInfo = Request<IHasNullQueryKey>(x => x.FooAsync("boom"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Single(queryParams);
            Assert.Null(queryParams[0].SerializeToString(null).First().Key);
        }

        [Fact]
        public void SerializesEmptystringQueryKeys()
        {
            var requestInfo = Request<IHasEmptystringQueryKey>(x => x.FooAsync("boom"));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Single(queryParams);
            Assert.Equal(String.Empty, queryParams[0].SerializeToString(null).First().Key);
        }

        [Fact]
        public void HandlesFormattedQueryParams()
        {
            var requestInfo = Request<IHasFormat>(x => x.FooAsync(11));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal("¤11.00", queryParams[0].SerializeToString(CultureInfo.InvariantCulture).First().Value);
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var requestInfo = Request<IHasFormat>(x => x.FooAsync(3));
            var formatProvider = new Mock<IFormatProvider>();

            var param = requestInfo.QueryParams.First();
            param.SerializeToString(formatProvider.Object);

            formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }

        [Fact]
        public void SerializesNullQueryValues()
        {
            var requestInfo = Request<INullableQueryParameters>(x => x.FooAsync(null, null, null));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(3, queryParams.Count);

            // This overlaps with a test in QueryParameterTests
            Assert.Empty(queryParams[0].SerializeToString(null));
            Assert.Empty(queryParams[1].SerializeToString(null));
            Assert.Empty(queryParams[2].SerializeToString(null));
        }

        [Fact]
        public void HandlesQueryProperty()
        {
            var requestInfo = Request<IHasPathQuery>(x =>
            {
                x.Foo = "woo";
                return x.FooAsync();
            });

            var queryProperties = requestInfo.QueryProperties.ToList();

            Assert.Single(queryProperties);

            var serialized = queryProperties[0].SerializeToString(null).ToList();
            Assert.Single(serialized);
            Assert.Equal("foo", serialized[0].Key);
            Assert.Equal("woo", serialized[0].Value);
        }

        [Fact]
        public void HandlesFormattedQueryProperty()
        {
            var requestInfo = Request<IHasFormattedPathQuery>(x =>
            {
                x.Woo = 10;
                return x.FooAsync();
            });

            var queryProperties = requestInfo.QueryProperties.ToList();

            Assert.Single(queryProperties);

            var serialized = queryProperties[0].SerializeToString(null).ToList();
            Assert.Single(serialized);
            Assert.Equal("Woo", serialized[0].Key);
            Assert.Equal("A", serialized[0].Value);
        }

        [Fact]
        public void HandlesQueryParameterNameUsingAttributeSetter()
        {
            var requestInfo = Request<IHasQueryParameterNameUsingAttributeSetter>(x => x.FooAsync("foo"));

            var queryParameters = requestInfo.QueryParams.ToList();

            Assert.Single(queryParameters);

            var serialized = queryParameters[0].SerializeToString(null).ToList();
            Assert.Single(serialized);
            Assert.Equal("customName", serialized[0].Key);
        }

        [Fact]
        public void HandlesQueryParameterNameUsingAttributeConstructor()
        {
            var requestInfo = Request<IHasQueryParameterNameUsingAttributeConstructor>(x => x.FooAsync("foo"));

            var queryParameters = requestInfo.QueryParams.ToList();

            Assert.Single(queryParameters);

            var serialized = queryParameters[0].SerializeToString(null).ToList();
            Assert.Single(serialized);
            Assert.Equal("customName", serialized[0].Key);
        }

        [Fact]
        public void HandlesSerializedQueryProperty()
        {
            var requestInfo = Request<IHasSerializedPathQuery>(x =>
            {
                x.Yay = "woop";
                return x.FooAsync();
            });

            var queryProperties = requestInfo.QueryProperties.ToList();

            Assert.Single(queryProperties);
            Assert.Equal(QuerySerializationMethod.Serialized, queryProperties[0].SerializationMethod);
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
