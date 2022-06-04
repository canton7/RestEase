using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class MultipleParameterAttributesTests : ImplementationFactoryTestsBase
    {
        public interface IHasMultipleParameterAttributes
        {
            [Get("/{bar}")]
            Task FooAsync([Query, Header("header1"), Body, Path, HttpRequestMessageProperty("prop1")] string bar);
        }

        public MultipleParameterAttributesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesMultipleParameterAttributes()
        {
            var requestInfo = this.Request<IHasMultipleParameterAttributes>(x => x.FooAsync("boom"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Single(requestInfo.HeaderParams);
            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Single(requestInfo.PathParams);
            Assert.Single(requestInfo.HttpRequestMessageProperties);
        }
    }
}
