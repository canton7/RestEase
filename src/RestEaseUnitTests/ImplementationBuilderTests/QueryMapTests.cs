using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class QueryMapTests
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

        public interface IHasEnumerableQueryMap
        {
            [Get("foo")]
            Task FooAsync([QueryMap] IDictionary<string, string[]> map);
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

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void AddsQueryMapToQueryParams()
        {
            var implementation = this.builder.CreateImplementation<IHasGenericQueryMap>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var queryMap = new Dictionary<string, string>()
            {
                { "foo", "bar" },
                { "bar", "yay" }
            };

            implementation.FooAsync(queryMap);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("bar", queryParam0.Value);

            var queryParam1 = requestInfo.QueryParams[1].SerializeToString().First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("yay", queryParam1.Value);
        }

        [Fact]
        public void AssignsEnumerableQueryMapToQueryParams()
        {
            var implementation = this.builder.CreateImplementation<IHasEnumerableQueryMap>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var queryMap = new Dictionary<string, string[]>()
            {
                { "foo", new[] {  "bar1", "bar2" } },
                { "bar", new[] {  "yay1", "yay2" } }
            };

            implementation.FooAsync(queryMap);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().ToArray();
            Assert.Equal("foo", queryParam0[0].Key);
            Assert.Equal("bar1", queryParam0[0].Value);
            Assert.Equal("foo", queryParam0[1].Key);
            Assert.Equal("bar2", queryParam0[1].Value);

            var queryParam1 = requestInfo.QueryParams[1].SerializeToString().ToArray();
            Assert.Equal("bar", queryParam1[0].Key);
            Assert.Equal("yay1", queryParam1[0].Value);
            Assert.Equal("bar", queryParam1[1].Key);
            Assert.Equal("yay2", queryParam1[1].Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<IHasGenericQueryMap>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } });

            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<IHasSerializedSerializationMethod>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } });

            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void HandlesNullQueryMap()
        {
            var implementation = this.builder.CreateImplementation<IHasGenericQueryMap>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(null);

            Assert.Equal(0, requestInfo.QueryParams.Count);
        }

        [Fact]
        public void ThrowsIfInvalidQueryMapType()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasInvalidQueryMap>(this.requester.Object));
        }

        [Fact]
        public void AllowsMoreThanOneQueryMap()
        {
            var implementation = this.builder.CreateImplementation<IHasTwoQueryMaps>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var queryMap1 = new Dictionary<string, string>()
            {
                { "foo", "bar" }
            };

            var queryMap2 = new Dictionary<string, string>()
            {
                { "foo", "yay" }
            };

            implementation.FooAsync(queryMap1, queryMap2);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("bar", queryParam0.Value);

            var queryParam1 = requestInfo.QueryParams[1].SerializeToString().First();
            Assert.Equal("foo", queryParam1.Key);
            Assert.Equal("yay", queryParam1.Value);
        }

        [Fact]
        public void DefaultQuerySerializationMethodIsSpecifiedBySerializationMethodsAttribute()
        {
            var implementation = this.builder.CreateImplementation<IHasNonOverriddenSerializationMethodsAttribute>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } });

            Assert.Equal(QuerySerializationMethod.Serialized, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var implementation = this.builder.CreateImplementation<IHasOverriddenSerializationMethodsAttribute>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new Dictionary<string, string>() { { "foo", "bar" } });

            Assert.Equal(QuerySerializationMethod.ToString, requestInfo.QueryParams[0].SerializationMethod);
        }
    }
}
