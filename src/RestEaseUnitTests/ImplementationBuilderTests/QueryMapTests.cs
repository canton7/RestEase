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

        //[Fact]
        //public void AssignsGenericQueryMap()
        //{
        //    var implementation = this.builder.CreateImplementation<IHasGenericQueryMap>(this.requester.Object);
        //    IRequestInfo requestInfo = null;

        //    this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
        //        .Callback((IRequestInfo r) => requestInfo = r)
        //        .Returns(Task.FromResult(false));

        //    // ExpandoObject implements IDictionary<string, object> but not IDictionary
        //    dynamic queryMap = new ExpandoObject();
        //    queryMap.foo = "bar";
        //    queryMap.baz = null;

        //    implementation.FooAsync(queryMap);

        //    Assert.Equal(queryMap, requestInfo.QueryMap);
        //}

        //[Fact]
        //public void AssignsNonGenericQueryMap()
        //{
        //    var implementation = this.builder.CreateImplementation<IHasNonGenericQueryMap>(this.requester.Object);
        //    IRequestInfo requestInfo = null;

        //    this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
        //        .Callback((IRequestInfo r) => requestInfo = r)
        //        .Returns(Task.FromResult(false));

        //    IDictionary queryMap = new Dictionary<string, string>()
        //    {
        //        { "foo", "bar" },
        //        { "baz", null },
        //    };

        //    implementation.FooAsync(queryMap);

        //    Assert.Equal(queryMap, requestInfo.QueryMap);
        //}

        [Fact]
        public void ThrowsIfInvalidQueryMapType()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasInvalidQueryMap>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMoreThanOneQueryMap()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasTwoQueryMaps>(this.requester.Object));
        }
    }
}
