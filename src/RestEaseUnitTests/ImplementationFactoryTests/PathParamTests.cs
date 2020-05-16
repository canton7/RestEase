using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
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

        public interface IHasDuplicatePathProperties
        {
            [Path("foo")]
            string Foo { get; set; }

            [Path("foo")]
            string Foo2 { get; set; }

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
            Task FooAsync([Path(Format = "C")] int foo);
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

        public interface ISerializedPathParam
        {
            [Get("{foo}")]
            Task FooAsync([Path(PathSerializationMethod.Serialized)] object foo);
        }

        [SerializationMethods(Path = PathSerializationMethod.Serialized)]
        public interface IHasNonOverriddenDefaultPathSerializationMethod
        {
            [Get("{foo}")]
            Task FooAsync([Path] string foo);
        }

        [SerializationMethods(Path = PathSerializationMethod.Serialized)]
        public interface IHasOverriddenDefaultPathSerializationMethod
        {
            [Get("{foo}")]
            Task FooAsync([Path(PathSerializationMethod.ToString)] string foo);
        }

        public interface IHasSerializedPathProperty
        {
            [Path(PathSerializationMethod.Serialized)]
            string Yay { get; set; }

            [Get("{Yay}")]
            Task FooAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void HandlesNullPathParams()
        {
            var requestInfo = this.Request<IPathParams>(x => x.DifferentParameterTypesAsync(null, null));

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
            var requestInfo = this.Request<IPathParams>(x => x.FooAsync("foo value", "bar value"));

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
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasPathParamInPathButNotParameters>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPathParamPresentInParametersButNotPath()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasPathParamInParametersButNotPath>(this.requester.Object));
        }

        [Fact]
        public void PathParamWithImplicitNameDoesNotFailValidation()
        {
            this.factory.CreateImplementation<IHasPathParamWithoutExplicitName>(this.requester.Object);
        }

        [Fact]
        public void ThrowsIfDuplicatePathParameters()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasDuplicatePathParams>(this.requester.Object));
        }

        [Fact]
        public void HandlesNullAndEmptyPaths()
        {
            // Do not throw
            this.factory.CreateImplementation<IHasEmptyGetParams>(this.requester.Object);
        }

        [Fact]
        public void HandlesBothGetAndQueryParams()
        {
            var requestInfo = this.Request<IHasBothPathAndQueryParams>(x => x.FooAsync("foovalue", "barvalue"));

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
            var requestInfo = this.Request<IHasPathProperty>(x =>
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
        public void ThrowsIfDuplicatePathProperties()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasDuplicatePathProperties>(this.requester.Object));
        }

        [Fact]
        public void DoesNotThrowIfPathPropertyAndParamHaveTheSameName()
        {
            this.Request<IHasBothPathPropertyAndPathParam>(x =>
            {
                x.Foo = "bar";
                return x.FooAsync("yay");
            });
        }

        [Fact]
        public void HandlesPathPropertyWithNoName()
        {
            var requestInfo = this.Request<IHasPathPropertyWithNoName>(x =>
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
            var requestInfo = this.Request<IHasFormattedPathProperty>(x =>
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
            var requestInfo = this.Request<IHasFormattedPathParam>(x => x.FooAsync(2));

            var pathParams = requestInfo.PathParams.ToList();
            Assert.Single(pathParams);

            var serialized = pathParams[0].SerializeToString(CultureInfo.InvariantCulture);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("¤2.00", serialized.Value);
        }

        [Fact]
        public void HandlesPathPropertiesWithNoUrlEncoding()
        {
            var requestInfo = this.Request<IHasPathPropertyWithNoUrlEncoding>(x =>
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
            var requestInfo = this.Request<IHasPathParamWithNoUrlEncoding>(x => x.FooAsync("a/b+c"));

            var pathParams = requestInfo.PathParams.ToList();
            Assert.Single(pathParams);

            Assert.False(pathParams[0].UrlEncode);
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var requestInfo = this.Request<IHasFormattedPathParam>(x => x.FooAsync(3));
            var formatProvider = new Mock<IFormatProvider>();

            var param = requestInfo.PathParams.First();
            param.SerializeToString(formatProvider.Object);

            formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var requestInfo = this.Request<ISerializedPathParam>(x => x.FooAsync("fizzbuzz"));

            Assert.Single(requestInfo.PathParams);
            Assert.Equal(PathSerializationMethod.Serialized, requestInfo.PathParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultPathSerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var requestInfo = this.Request<IHasNonOverriddenDefaultPathSerializationMethod>(x => x.FooAsync("buzz"));

            Assert.Single(requestInfo.PathParams);
            Assert.Equal(PathSerializationMethod.Serialized, requestInfo.PathParams.First().SerializationMethod);
        }

        [Fact]
        public void DefaultPathSerializationMethodCanBeOverridden()
        {
            var requestInfo = this.Request<IHasOverriddenDefaultPathSerializationMethod>(x => x.FooAsync("foo"));

            Assert.Single(requestInfo.PathParams);
            Assert.Equal(PathSerializationMethod.ToString, requestInfo.PathParams.First().SerializationMethod);
        }

        [Fact]
        public void HandlesSerializedPathProperty()
        {
            var requestInfo = this.Request<IHasSerializedPathProperty>(x =>
            {
                x.Yay = "woopdidoo";
                return x.FooAsync();
            });

            var pathProperties = requestInfo.PathProperties.ToList();

            Assert.Single(pathProperties);
            Assert.Equal(PathSerializationMethod.Serialized, pathProperties[0].SerializationMethod);
        }

        private IRequestInfo Request<T>(Func<T, Task> selector)
        {
            var implementation = this.factory.CreateImplementation<T>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            selector(implementation);

            return requestInfo;
        }
    }
}
