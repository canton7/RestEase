using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            Task DifferentParameterTypesAsync([Path] object foo, [Path] int? bar);
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

        public interface IHasFormattedPathProperty
        {
            [Path("foo", Format = "X")]
            int Foo { get; set; }

            [Get("path/{foo}")]
            Task FooAsync();
        }

        public interface IHasFormattedPathParam
        {
            [Get("path/{foo}")]
            Task FooAsync([Path(Format = "D2")] int foo);
        }

        public interface IHasPathPropertyWithNoUrlEncoding
        {
            [Path("foo", UrlEncode = false)]
            string Foo { get; set; }

            [Get("path/{foo}")]
            Task FooAsync();
        }

        public interface IHasPathParamWithNoUrlEncoding
        {
            [Get("path/{foo}")]
            Task FooAsync([Path(UrlEncode = false)] string foo);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

        [Fact]
        public void HandlesNullPathParams()
        {
            var requestInfo = Request<IPathParams>(x => x.DifferentParameterTypesAsync(null, null));

            var pathParams = requestInfo.PathParams.ToList();

            Assert.Equal(2, pathParams.Count);

            var serialized0 = pathParams[0].SerializeToString(null);
            Assert.Equal("foo", serialized0.Key);
            Assert.Null(serialized0.Value);

            var serialized1 = pathParams[1].SerializeToString(null);
            Assert.Equal("bar", serialized1.Key);
            Assert.Null(serialized1.Value);
        }

        [Fact]
        public void NullPathParamsAreRenderedAsEmpty()
        {
            var requestInfo = Request<IPathParams>(x => x.FooAsync("foo value", "bar value"));

            var pathParams = requestInfo.PathParams.ToList();

            Assert.Equal(2, pathParams.Count);

            var serialized0 = pathParams[0].SerializeToString(null);
            Assert.Equal("foo", serialized0.Key);
            Assert.Equal("foo value", serialized0.Value);

            var serialized1 = pathParams[1].SerializeToString(null);
            Assert.Equal("bar", serialized1.Key);
            Assert.Equal("bar value", serialized1.Value);
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

            Assert.Single(pathParams);

            var serialized = pathParams[0].SerializeToString(null);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("foovalue", serialized.Value);

            Assert.Single(queryParams);

            var queryParam = queryParams[0].SerializeToString(null).First();
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

            Assert.Single(pathProperties);

            var serialized = pathProperties[0].SerializeToString(null);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("bar", serialized.Value);
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

            Assert.Single(pathProperties);

            var serialized = pathProperties[0].SerializeToString(null);
            Assert.Equal("Foo", serialized.Key);
            Assert.Equal("bar", serialized.Value);
        }

        [Fact]
        public void HandlesFormattedPathProperties()
        {
            var requestInfo = Request<IHasFormattedPathProperty>(x =>
            {
                x.Foo = 10;
                return x.FooAsync();
            });

            var pathProperties = requestInfo.PathProperties.ToList();
            Assert.Single(pathProperties);

            var serialized = pathProperties[0].SerializeToString(null);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("A", serialized.Value);
        }

        [Fact]
        public void HandlesFormattedPathParams()
        {
            var requestInfo = Request<IHasFormattedPathParam>(x =>
            {
                return x.FooAsync(2);
            });

            var pathParams = requestInfo.PathParams.ToList();
            Assert.Single(pathParams);

            var serialized = pathParams[0].SerializeToString(null);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("02", serialized.Value);
        }

        [Fact]
        public void HandlesPathPropertiesWithNoUrlEncoding()
        {
            var requestInfo = Request<IHasPathPropertyWithNoUrlEncoding>(x =>
            {
                x.Foo = "a/b+c";
                return x.FooAsync();
            });

            var pathProperties = requestInfo.PathProperties.ToList();
            Assert.Single(pathProperties);

            Assert.False(pathProperties[0].UrlEncode);
        }

        [Fact]
        public void HandlesPathParamsWithNoUrlEncoding()
        {
            var requestInfo = Request<IHasPathParamWithNoUrlEncoding>(x => x.FooAsync("a/b+c"));

            var pathParams = requestInfo.PathParams.ToList();
            Assert.Single(pathParams);

            Assert.False(pathParams[0].UrlEncode);
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var requestInfo = Request<IHasFormattedPathParam>(x => x.FooAsync(3));
            var formatProvider = new Mock<IFormatProvider>();

            var param = requestInfo.PathParams.First();
            param.SerializeToString(formatProvider.Object);

            formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
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
