using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestEase;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CA1050 // Declare types in namespaces
public interface IApiOutsideNamespace
{
    [Get]
    Task FooAsync();
}
#pragma warning restore CA1050 // Declare types in namespaces

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class EdgeCaseTests : ImplementationFactoryTestsBase
    {
        public interface IHasNrts
        {
            [Query]
            string? Foo { get; set; }

            [Get]
            Task<string?> FooAsync(string? arg);
        }

        public EdgeCaseTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesImplementationOutsideOfNamespace()
        {
            this.Request<IApiOutsideNamespace>(x => x.FooAsync());
        }

        [Fact]
        public void DoesNotGenerateNrtRelatedWarnings()
        {
            // We're looking for compiler warnings
            this.CreateImplementation<IHasNrts>();
        }
    }
}
