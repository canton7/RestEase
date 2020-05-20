using RestEase;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class AllowAnyStatusCodeTests : ImplementationFactoryTestsBase
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

        public AllowAnyStatusCodeTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void DefaultsToFalse()
        {
            var requestInfo = this.Request<IHasNoAllowAnyStatusCode>(x => x.FooAsync());

            Assert.False(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void RespectsAllowAnyStatusCodeOnMethod()
        {
            var requestInfo = this.Request<IHasMethodWithAllowAnyStatusCode>(x => x.FooAsync());

            Assert.True(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void RespectsAllowAnyStatusCodeOnInterface()
        {
            var requestInfo = this.Request<IHasAllowAnyStatusCode>(x => x.NoAttributeAsync());

            Assert.True(requestInfo.AllowAnyStatusCode);
        }

        [Fact]
        public void AllowsAllowAnyStatusCodeOnMethodToOverrideThatOnInterface()
        {
            var requestInfo = this.Request<IHasAllowAnyStatusCode>(x => x.HasAttributeAsync());

            Assert.False(requestInfo.AllowAnyStatusCode);
        }
    }
}
