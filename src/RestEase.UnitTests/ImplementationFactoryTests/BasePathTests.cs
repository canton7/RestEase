using RestEase.Implementation;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class BasePathTests : ImplementationFactoryTestsBase
    {
        public interface IHasNoBasePath
        {
            [Get("path")]
            Task FooAsync();
        }

        [BasePath("foo/bar/baz")]
        public interface IHasSimpleBasePath
        {
            [Get("path")]
            Task FooAsync();
        }

        [BasePath("foo/{bar}/baz")]
        public interface IHasBasePathWithPlaceholderWithoutProperty
        {
            [Get("{bar}")]
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

        public BasePathTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void DefaultsToNull()
        {
            var requestInfo = this.Request<IHasNoBasePath>(x => x.FooAsync());

            Assert.Null(requestInfo.BasePath);
        }

        [Fact]
        public void ForwardsSimpleBasePath()
        {
            var requestInfo = this.Request<IHasSimpleBasePath>(x => x.FooAsync());

            Assert.Equal("foo/bar/baz", requestInfo.BasePath);
        }

        [Fact]
        public void ThrowsIfBasePathPlaceholderMissingPathProperty()
        {
            VerifyDiagnostics<IHasBasePathWithPlaceholderWithoutProperty>(
                // (1,10): Error REST001: Unable to find a [Path("bar")] property for the path placeholder '{bar}' in base path 'foo/{bar}/baz'
                // BasePath("foo/{bar}/baz")
                Diagnostic(DiagnosticCode.MissingPathPropertyForBasePathPlaceholder, @"BasePath(""foo/{bar}/baz"")").WithLocation(1, 10)
            );
        }

        [Fact]
        public void FowardsBasePathWithPlaceholder()
        {
            var requestInfo = this.Request<IHasBasePathWithPlaceholder>(x => x.FooAsync());

            Assert.Equal("foo/{bar}/baz", requestInfo.BasePath);
        }
    }
}
