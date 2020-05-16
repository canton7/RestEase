using Moq;
using RestEase;
using RestEase.Implementation;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class AllowAnyStatusCodeTests
    {
        public interface IHasNoAllowAnyStatusCode
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasMethodWithAllowAnyStatusCode
        {
            [AllowAnyStatusCode]
            [Get("foo")]
            Task FooAsync();
        }

        [AllowAnyStatusCode]
        public interface IHasAllowAnyStatusCode
        {
            [Get("foo")]
            Task NoAttributeAsync();

            [Get("bar")]
            [AllowAnyStatusCode(false)]
            Task HasAttributeAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void DefaultsToFalse()
        {
            var implementation = this.factory.CreateImplementation<IHasNoAllowAnyStatusCode>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.False(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void RespectsAllowAnyStatusCodeOnMethod()
        {
            var implementation = this.factory.CreateImplementation<IHasMethodWithAllowAnyStatusCode>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.True(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void RespectsAllowAnyStatusCodeOnInterface()
        {
            var implementation = this.factory.CreateImplementation<IHasAllowAnyStatusCode>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.NoAttributeAsync();

            Assert.True(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void AllowsAllowAnyStatusCodeOnMethodToOverrideThatOnInterface()
        {
            var implementation = this.factory.CreateImplementation<IHasAllowAnyStatusCode>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.HasAttributeAsync();

            Assert.False(requestInfo.AllowAnyStatusCode);
        }
    }
}
