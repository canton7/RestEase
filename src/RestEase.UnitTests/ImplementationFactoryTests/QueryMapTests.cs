using Moq;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class QueryMapTests : ImplementationFactoryTestsBase
    {
        public interface IHasInvalidQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] string map);
        }

        public interface IHasTwoQueryMaps
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, string> map, [QueryMap] IDictionary<string, string> map2);
        }

        public interface IHasGenericQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, string> map);
        }

        public interface IHasArrayQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, string[]> map);
        }

        public interface IHasEnumerableQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, List<string>> map);
        }

        public interface IHasSerializedSerializationMethod
        {
            [Get("foo")]
            Task FooAsync([QueryMap(QuerySerializationMethod.Serialized)] IDictionary<string, string> map);
        }

        [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
        public interface IHasNonOverriddenSerializationMethodsAttribute
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, string> map);
        }

        [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
        public interface IHasOverriddenSerializationMethodsAttribute
        {
            [Get("foo")]
            Task FooAsync([QueryMap(QuerySerializationMethod.ToString)] IDictionary<string, string> map);
        }

        public interface IHasObjectQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, object> map);
        }

        public interface IHasGenericDictionaryQueryMap
        {
            [Get]
            Task FooAsync<TDictionary>([QueryMap] TDictionary map) where TDictionary : IDictionary<string, string>;
        }

        public interface IHasGenericCollectionQueryMap
        {
            [Get]
            Task FooAsync<TCollection>([QueryMap] IDictionary<string, TCollection> map) where TCollection : IEnumerable<string>;
        }

        public QueryMapTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void AddsQueryMapToQueryParams()
        {
            var queryMap = new Dictionary<string, string>()
            {
                { "foo", "bar" },
                { "bar", "yay" }
            };

            var requestInfo = this.Request<IHasGenericQueryMap>(x => x.FooAsync(queryMap));

            var queryParams = requestInfo.QueryParams.ToList();

            var queryParam0 = queryParams[0].SerializeToString(null).First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("bar", queryParam0.Value);

            var queryParam1 = queryParams[1].SerializeToString(null).First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("yay", queryParam1.Value);
        }

        [Fact]
        public void AssignsArrayQueryMapToQueryParams()
        {
            var queryMap = new Dictionary<string, string[]>()
            {
                { "foo", new[] {  "bar1", "bar2" } },
                { "bar", new[] {  "yay1", "yay2" } }
            };

            var requestInfo = this.Request<IHasArrayQueryMap>(x => x.FooAsync(queryMap));
            
            var queryParams = requestInfo.QueryParams.ToList();

            var queryParam0 = queryParams[0].SerializeToString(null).ToArray();
            Assert.Equal("foo", queryParam0[0].Key);
            Assert.Equal("bar1", queryParam0[0].Value);
            Assert.Equal("foo", queryParam0[1].Key);
            Assert.Equal("bar2", queryParam0[1].Value);

            var queryParam1 = queryParams[1].SerializeToString(null).ToArray();
            Assert.Equal("bar", queryParam1[0].Key);
            Assert.Equal("yay1", queryParam1[0].Value);
            Assert.Equal("bar", queryParam1[1].Key);
            Assert.Equal("yay2", queryParam1[1].Value);
        }

        [Fact]
        public void AssignsEnumerableQueryMapToQueryParams()
        {
            var queryMap = new Dictionary<string, List<string>>()
            {
                { "foo", new List<string>() { "bar1", "bar2" } },
                { "bar", new List<string>() { "yay1", "yay2" } }
            };

            var requestInfo = this.Request<IHasEnumerableQueryMap>(x => x.FooAsync(queryMap));

            var queryParams = requestInfo.QueryParams.ToList();

            var queryParam0 = queryParams[0].SerializeToString(null).ToArray();
            Assert.Equal("foo", queryParam0[0].Key);
            Assert.Equal("bar1", queryParam0[0].Value);
            Assert.Equal("foo", queryParam0[1].Key);
            Assert.Equal("bar2", queryParam0[1].Value);

            var queryParam1 = queryParams[1].SerializeToString(null).ToArray();
            Assert.Equal("bar", queryParam1[0].Key);
            Assert.Equal("yay1", queryParam1[0].Value);
            Assert.Equal("bar", queryParam1[1].Key);
            Assert.Equal("yay2", queryParam1[1].Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var requestInfo = this.Request<IHasGenericQueryMap>(x => x.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } }));

            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var requestInfo = this.Request< IHasSerializedSerializationMethod >(x => x.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } }));

            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void HandlesNullQueryMap()
        {
            var requestInfo = this.Request<IHasGenericQueryMap>(x => x.FooAsync(null));

            Assert.Empty(requestInfo.QueryParams);
        }

        [Fact]
        public void ThrowsIfInvalidQueryMapType()
        {
            VerifyDiagnostics<IHasInvalidQueryMap>(
                // (4,27): Error REST013: [QueryMap] parameter is not of the type IDictionary or IDictionary<TKey, TValue> (or their descendents)
                // [QueryMap] string map
                Diagnostic(DiagnosticCode.QueryMapParameterIsNotADictionary, "[QueryMap] string map").WithLocation(4, 27)
            );
        }

        [Fact]
        public void AllowsMoreThanOneQueryMap()
        {
            var queryMap1 = new Dictionary<string, string>()
            {
                { "foo", "bar" }
            };

            var queryMap2 = new Dictionary<string, string>()
            {
                { "foo", "yay" }
            };

            var requestInfo = this.Request<IHasTwoQueryMaps>(x => x.FooAsync(queryMap1, queryMap2));

            var queryParams = requestInfo.QueryParams.ToList();

            var queryParam0 = queryParams[0].SerializeToString(null).First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("bar", queryParam0.Value);

            var queryParam1 = queryParams[1].SerializeToString(null).First();
            Assert.Equal("foo", queryParam1.Key);
            Assert.Equal("yay", queryParam1.Value);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsAttribute()
        {
            var requestInfo = this.Request<IHasNonOverriddenSerializationMethodsAttribute>(x =>
                x.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } }));

            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var requestInfo = this.Request<IHasOverriddenSerializationMethodsAttribute>(x =>
                x.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } }));

            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams.First().SerializationMethod);
        }

        [Fact]
        public void IteratesQueryMapEnumerableObjectValue()
        {
            var requestInfo = this.Request<IHasObjectQueryMap>(x => x.FooAsync(new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "baz", new[] { "a", "b", "c" } }
            }));

            var queryParams = requestInfo.QueryParams.ToList();

            var queryParam0 = queryParams[0].SerializeToString(null).Single();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("bar", queryParam0.Value);

            var queryParam1 = queryParams[1].SerializeToString(null).ToArray();
            Assert.Equal("baz", queryParam1[0].Key);
            Assert.Equal("a", queryParam1[0].Value);

            Assert.Equal("baz", queryParam1[1].Key);
            Assert.Equal("b", queryParam1[1].Value);

            Assert.Equal("baz", queryParam1[2].Key);
            Assert.Equal("c", queryParam1[2].Value);
        }

        [Fact]
        public void HandlesGenericDictionary()
        {
            var requestInfo = this.Request<IHasGenericDictionaryQueryMap>(x => x.FooAsync(new Dictionary<string, string>()
            {
                { "key", "value" }
            }));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Single(queryParams);
            var queryParam0 = queryParams[0].SerializeToString(null).ToList();
            Assert.Single(queryParam0);
            Assert.Equal("key", queryParam0[0].Key);
            Assert.Equal("value", queryParam0[0].Value);
        }

        [Fact]
        public void HandlesDictionaryWithGenericCollection()
        {
            var requestInfo = this.Request<IHasGenericCollectionQueryMap>(x => x.FooAsync(new Dictionary<string, string[]>()
            {
                { "key", new[] { "v1", "v2" } },
            }));

            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Single(queryParams);
            var queryParam0 = queryParams[0].SerializeToString(null).ToList();
            Assert.Equal(2, queryParam0.Count);
            Assert.Equal("key", queryParam0[0].Key);
            Assert.Equal("v1", queryParam0[0].Value);
            Assert.Equal("key", queryParam0[1].Key);
            Assert.Equal("v2", queryParam0[1].Value);
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var queryMap = new Dictionary<string, object>()
            {
                { "foo", 3.3 }
            };
            var requestInfo = this.Request<IHasObjectQueryMap>(x => x.FooAsync(queryMap));

            var formatProvider = new Mock<IFormatProvider>();

            var param = requestInfo.QueryParams.First();
            param.SerializeToString(formatProvider.Object);

            formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }
    }
}
