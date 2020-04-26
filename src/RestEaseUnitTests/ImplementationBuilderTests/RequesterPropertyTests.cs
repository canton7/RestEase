using Moq;
using RestEase;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class RequesterPropertyTests
    {
        public interface IHasRequesterProperty
        {
            IRequester Requester { get; }
        }

        [Fact]
        public void HandlesRequesterProperty()
        {
            var requester = new Mock<IRequester>().Object;
            var client = RestClient.For<IHasRequesterProperty>(requester);
            Assert.Equal(requester, client.Requester);
        }
    }
}
