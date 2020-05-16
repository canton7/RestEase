using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class ImplementationBuilderTests
    {
        [Fact]
        public void ImplementationBuildInstanceMustBeASingleton()
        {
            Assert.Same(EmitImplementationFactory.Instance, EmitImplementationFactory.Instance);
        }
    }
}
