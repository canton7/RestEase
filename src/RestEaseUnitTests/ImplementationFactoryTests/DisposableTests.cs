using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class DisposableTests
    {
        public interface IDisposableApi : IDisposable
        {
            [Get("foo")]
            Task FooAsync();
        }

        private readonly ImplementationFactory builder = ImplementationFactory.Instance;

        [Fact]
        public void DisposingDisposableImplementationDisposesRequester()
        {
            var requester = new Mock<IRequester>();
            var implementation = this.builder.CreateImplementation<IDisposableApi>(requester.Object);
            implementation.Dispose();
            requester.Verify(x => x.Dispose());
        }
    }
}
