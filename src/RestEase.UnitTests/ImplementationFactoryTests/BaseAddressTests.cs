using RestEase.Implementation;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class BaseAddressTests : ImplementationFactoryTestsBase
    {
        public interface IHasNoBaseAddress
        {
            [Get("path")]
            Task FooAsync();
        }

        [BaseAddress("http://foo/bar/baz")]
        public interface IHasSimpleBaseAddress
        {
            [Get("path")]
            Task FooAsync();
        }

        [BaseAddress("http://foo/{bar}/baz")]
        public interface IHasBaseAddressWithPlaceholderWithoutProperty
        {
            [Get("{bar}")]
            Task FooAsync([Path] string bar);
        }

        [BaseAddress("http://foo/{bar}/baz")]
        public interface IHasBaseAddressWithPlaceholder
        {
            [Path("bar")]
            string Bar { get; set; }

            [Get]
            Task FooAsync();
        }

        [BaseAddress("foo")]
        public interface IHasRelativeBaseAddress
        {
        }

        public BaseAddressTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void DefaultsToNull()
        {
            var requestInfo = this.Request<IHasNoBaseAddress>(x => x.FooAsync());

            Assert.Null(requestInfo.BaseAddress);
        }

        [Fact]
        public void ForwardsSimpleBaseAddress()
        {
            var requestInfo = this.Request<IHasSimpleBaseAddress>(x => x.FooAsync());

            Assert.Equal("http://foo/bar/baz", requestInfo.BaseAddress);
        }

        [Fact]
        public void ThrowsIfBaseAddressPlaceholderMissingAddressProperty()
        {
            this.VerifyDiagnostics<IHasBaseAddressWithPlaceholderWithoutProperty>(
                // (1,10): Error REST035: Unable to find a [Path("bar")] property for the path placeholder '{bar}' in base address 'http://foo/{bar}/baz'
                // BaseAddress("http://foo/{bar}/baz")
                Diagnostic(DiagnosticCode.MissingPathPropertyForBaseAddressPlaceholder, @"BaseAddress(""http://foo/{bar}/baz"")").WithLocation(1, 10)
            );
        }

        [Fact]
        public void FowardsBaseAddressWithPlaceholder()
        {
            var requestInfo = this.Request<IHasBaseAddressWithPlaceholder>(x => x.FooAsync());

            Assert.Equal("http://foo/{bar}/baz", requestInfo.BaseAddress);
        }

        [Fact]
        public void ThrowsIfBaseAddressIsNotAbsolute()
        {
            this.VerifyDiagnostics<IHasRelativeBaseAddress>(
                // (1,10): Error REST036: Base address 'foo' must be an absolute URI
                // BaseAddress("foo")
                Diagnostic(DiagnosticCode.BaseAddressMustBeAbsolute, @"BaseAddress(""foo"")").WithLocation(1, 10)
            );
        }
    }
}
