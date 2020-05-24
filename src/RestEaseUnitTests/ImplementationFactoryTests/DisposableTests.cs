using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class DisposableTests : ImplementationFactoryTestsBase
    {
        public interface IDisposableApi : IDisposable
        {
            [Get("foo")]
            Task FooAsync();
        }

        public DisposableTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void DisposingDisposableImplementationDisposesRequester()
        {
            var implementation = this.CreateImplementation<IDisposableApi>();

            this.Requester.Setup(x => x.Dispose()).Verifiable();
            implementation.Dispose();
            this.Requester.Verify();
        }
    }
}
