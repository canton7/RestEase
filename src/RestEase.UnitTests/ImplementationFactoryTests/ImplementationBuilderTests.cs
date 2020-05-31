using RestEase.Implementation;
using Xunit;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class ImplementationBuilderTests
    {
        [Fact]
        public void ImplementationFactoryInstanceMustBeASingleton()
        {
            Assert.Same(ImplementationFactory.Instance, ImplementationFactory.Instance);
        }
    }
}
