using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class BasePathTests
    {
        public interface IHasNoBasePath
        {
            [Get("foo")]
            Task FooAsync();
        }

        [BasePath("foo/bar/baz")]
        public interface IHasSimpleBasePath
        {
            [Get("foo")]
            Task FooAsync();
        }

        [BasePath("foo/{bar}/baz")]
        public interface IHasBasePathWithPlaceholderWithoutProperty
        {
            [Get("foo")]
            Task FooAsync([Path] string bar);
        }

        [BasePath("foo/{bar}/baz")]
        public interface IHasBasePathWithPlaceholder
        {
            [Path("bar")]
            string Bar { get; set; }

            [Get]
            Task FooAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void DefaultsToNull()
        {
            var implementation = this.factory.CreateImplementation<IHasNoBasePath>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Null(requestInfo.BasePath);
        }

        [Fact]
        public void ForwardsSimpleBasePath()
        {
            var implementation = this.factory.CreateImplementation<IHasSimpleBasePath>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Equal("foo/bar/baz", requestInfo.BasePath);
        }

        [Fact]
        public void ThrowsIfPlaceholderMissingPathProperty()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasBasePathWithPlaceholderWithoutProperty>(this.requester.Object));
        }

        [Fact]
        public void FowardsBasePathWithPlaceholder()
        {
            var implementation = this.factory.CreateImplementation<IHasBasePathWithPlaceholder>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Equal("foo/{bar}/baz", requestInfo.BasePath);
        }
    }
}
