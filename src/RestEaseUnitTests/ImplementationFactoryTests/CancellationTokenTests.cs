using RestEase;
using RestEase.Implementation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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

        public CancellationTokenTests(ITestOutputHelper output) : base(output) { }

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
            this.VerifyDiagnostics<ITwoCancellationTokens>(
                // (4,27): Error REST001: Method 'YayAsync': only a single CancellationToken parameter is allowed, found a duplicate parameter 'cancellationToken2'
                // CancellationToken cancellationToken1
                Diagnostic(DiagnosticCode.MultipleCancellationTokenParameters, "CancellationToken cancellationToken1").WithLocation(4, 27).WithLocation(4, 65)
            );
        }
    }
}
