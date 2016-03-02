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

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void HandlesNullPathParams()
        {
            var implementation = this.builder.CreateImplementation<IPathParams>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.DifferentParaneterTypesAsync(null, null);

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
            var implementation = this.builder.CreateImplementation<IPathParams>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("foo value", "bar value");

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
    }
}
