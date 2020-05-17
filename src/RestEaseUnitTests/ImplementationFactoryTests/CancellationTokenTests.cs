using Moq;
using RestEase;
using RestEase.Implementation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class CancellationTokenTests : ImplementationFactoryTestsBase
    {
        public interface ICancellationTokenOnlyNoReturn
        {
            [Get("baz")]
            Task BazAsync(CancellationToken cancellationToken);
        }

        public interface ITwoCancellationTokens
        {
            [Get("yay")]
            Task YayAsync(CancellationToken cancellationToken1, CancellationToken cancellationToken2);
        }

        [Fact]
        public void CancellationTokenOnlyNoReturnCallsCorrectly()
        {
            var cts = new CancellationTokenSource();
            var requestInfo = this.Request<ICancellationTokenOnlyNoReturn>(x => x.BazAsync(cts.Token));

            Assert.Equal(cts.Token, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Empty(requestInfo.QueryParams);
            Assert.Equal("baz", requestInfo.Path);
        }

        [Fact]
        public void TwoCancellationTokensThrows()
        {
#if SOURCE_GENERATOR
            var diagnostics = this.GetDiagnostics<ITwoCancellationTokens>();
            Assert.Single(diagnostics);
#else
            Assert.Throws<ImplementationCreationException>(() => this.CreateImplementation<ITwoCancellationTokens>());
#endif
        }
    }
}
