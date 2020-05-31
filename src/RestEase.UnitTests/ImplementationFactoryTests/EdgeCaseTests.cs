using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestEase;
using Xunit;
using Xunit.Abstractions;

public interface IApiOutsideNamespace
{
    [Get]
    Task FooAsync();
}

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class EdgeCaseTests : ImplementationFactoryTestsBase
    {
        public EdgeCaseTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesImplementationOutsideOfNamespace()
        {
            this.Request<IApiOutsideNamespace>(x => x.FooAsync());
        }
    }
}
