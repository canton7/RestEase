using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class PathParamTests
    {
        public interface IPathParams
        {
            [Get("foo/{foo}/{bar}")]
            Task FooAsync([Path] string foo, [Path("bar")] string bar);

            [Get("foo/{foo}/{bar}")]
            Task DifferentParaneterTypesAsync([Path] object foo, [Path] int? bar);
        }

        public interface IHasPathParamInPathButNotParameters
        {
            [Get("foo/{bar}/{baz}")]
            Task FooAsync([Path("bar")] string bar);
        }

        public interface IHasPathParamInParametersButNotPath
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path("bar")] string path, [Path("baz")] string baz);
        }

        public interface IHasPathParamWithoutExplicitName
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path] string bar);
        }

        public interface IHasDuplicatePathParams
        {
            [Get("foo/{bar}")]
            Task FooAsync([Path] string bar, [Path("bar")] string yay);
        }

        public interface IHasEmptyGetParams
        {
            [Get]
            Task NullParamAsync();

            [Get("")]
            Task EmptyParamAsync();

            [Get("/")]
            Task SlashParamAsync();
        }

        public interface IHasBothPathAndQueryParams
        {
            [Get("/test/{foo}/test2")]
            Task FooAsync([Path] string foo, [Query("bar")] string bar);
        }

        public interface IHasPathProperty
        {
            [Path("foo")]
            string Foo { get; set; }

            [Get("{foo}")]
            Task FooAsync();
        }

        public interface IHasPathPropertyWithNoName
        {
            [Path]
            string Foo { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasBothPathPropertyAndPathParam
        {
            [Path("foo")]
            string Foo { get; set; }

            [Get("{foo}")]
            Task FooAsync([Path] string foo);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void HandlesNullPathParams()
        {
            var requestInfo = Request<IPathParams>(x => x.DifferentParaneterTypesAsync(null, null));

            var pathParams = requestInfo.PathParams.ToList();

            Assert.Equal(2, pathParams.Count);

            Assert.Equal("foo", pathParams[0].Key);
            Assert.Equal(null, pathParams[0].Value);

            Assert.Equal("bar", pathParams[1].Key);
            Assert.Equal(null, pathParams[1].Value);
        }

        [Fact]
        public void NullPathParamsAreRenderedAsEmpty()
        {
            var requestInfo = Request<IPathParams>(x => x.FooAsync("foo value", "bar value"));

            var pathParams = requestInfo.PathParams.ToList();

            Assert.Equal(2, pathParams.Count);

            Assert.Equal("foo", pathParams[0].Key);
            Assert.Equal("foo value", pathParams[0].Value);

            Assert.Equal("bar", pathParams[1].Key);
            Assert.Equal("bar value", pathParams[1].Value);
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

        [Fact]
        public void ThrowsIfDuplicatePathParameters()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasDuplicatePathParams>(this.requester.Object));
        }

        [Fact]
        public void HandlesNullAndEmptyPaths()
        {
            // Do not throw
            this.builder.CreateImplementation<IHasEmptyGetParams>(this.requester.Object);
        }

        [Fact]
        public void HandlesBothGetAndQueryParams()
        {
            var requestInfo = Request<IHasBothPathAndQueryParams>(x => x.FooAsync("foovalue", "barvalue"));

            var pathParams = requestInfo.PathParams.ToList();
            var queryParams = requestInfo.QueryParams.ToList();

            Assert.Equal(1, pathParams.Count);

            Assert.Equal("foo", pathParams[0].Key);
            Assert.Equal("foovalue", pathParams[0].Value);

            Assert.Equal(1, queryParams.Count);

            var queryParam = queryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam.Key);
            Assert.Equal("barvalue", queryParam.Value);
        }

        [Fact]
        public void HandlesPathProperty()
        {
            var requestInfo = Request<IHasPathProperty>(x =>
            {
                x.Foo = "bar";
                return x.FooAsync();
            });

            var pathProperties = requestInfo.PathProperties.ToList();

            Assert.Equal(1, pathProperties.Count);
            Assert.Equal("foo", pathProperties[0].Key);
            Assert.Equal("bar", pathProperties[0].Value);
        }

        [Fact]
        public void DoesNotThrowIfPathPropertyAndParamHaveTheSameName()
        {
            Request<IHasBothPathPropertyAndPathParam>(x =>
            {
                x.Foo = "bar";
                return x.FooAsync("yay");
            });
        }

        [Fact]
        public void HandlesPathPropertyWithNoName()
        {
            var requestInfo = Request<IHasPathPropertyWithNoName>(x =>
            {
                x.Foo = "bar";
                return x.FooAsync();
            });

            var pathProperties = requestInfo.PathProperties.ToList();

            Assert.Equal(1, pathProperties.Count);
            Assert.Equal("Foo", pathProperties[0].Key);
            Assert.Equal("bar", pathProperties[0].Value);
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
