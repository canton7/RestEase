using System.Threading.Tasks;
using RestEase.Implementation;
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

        public interface IHasMethodParameterWithConflictAttributes
        {
            [Get]
            Task FooAsync([Query, RawQueryString] string foo);
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

        [Fact]
        public void ThrowsIfQueryAttributeWithRawQueryStringAttribute()
        {
            VerifyDiagnostics<IHasMethodParameterWithConflictAttributes>(
                // (4,27): Error REST040: Method 'FooAsync': [Query] parameter must not specified along with [RawQueryString]
                // [Query, RawQueryString] string foo
                Diagnostic(DiagnosticCode.QueryConflictWithRawQueryString, @"[Query, RawQueryString] string foo")
                    .WithLocation(4, 27)
            );
        }
    }
}
