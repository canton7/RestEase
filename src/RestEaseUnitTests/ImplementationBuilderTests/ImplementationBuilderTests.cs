using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class ImplementationBuilderTests
    {
        [Fact]
        public void ImplementationBuildInstanceMustBeASingleton()
        {
            Assert.Same(ImplementationBuilder.Instance, ImplementationBuilder.Instance);
        }
    }
}
