using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class RequesterPropertyTests : ImplementationFactoryTestsBase
    {
        public interface IHasRequesterProperty
        {
            IRequester Requester { get; }
        }

        public interface IHasBadlyNamedRequesterProperty
        {
#pragma warning disable IDE1006 // Naming Styles
            IRequester @event { get; }
#pragma warning restore IDE1006 // Naming Styles
        }

        public interface IHasSet
        {
            IRequester Requester { get; set; }
        }

        public interface IHasNoGet
        {
            IRequester Requester { set; }
        }

        public interface ITwoRequesterProperties
        {
            IRequester Requester1 { get; }
            IRequester Requester2 { get; }
        }

        public RequesterPropertyTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesRequesterProperty()
        {
            var implementation = this.CreateImplementation<IHasRequesterProperty>();
            Assert.Equal(this.Requester.Object, implementation.Requester);
        }

        [Fact]
        public void HandlesBadlyNamedRequesterProperty()
        {
            var implementation = this.CreateImplementation<IHasBadlyNamedRequesterProperty>();
            Assert.Equal(this.Requester.Object, implementation.@event);
        }

        [Fact]
        public void ThrowsIfHasSet()
        {
            this.VerifyDiagnostics<IHasSet>(
                // (3,24): Error REST016: Property must have a getter but not a setter
                // Requester
                Diagnostic(DiagnosticCode.PropertyMustBeReadOnly, "Requester").WithLocation(3, 24)
            );
        }

        [Fact]
        public void ThrowsIfHasNoGet()
        {
            this.VerifyDiagnostics<IHasNoGet>(
                // (3,24): Error REST016: Property must have a getter but not a setter
                // Requester
                Diagnostic(DiagnosticCode.PropertyMustBeReadOnly, "Requester").WithLocation(3, 24)
            );
        }

        [Fact]
        public void ThrowsIfTwoRequesters()
        {
            this.VerifyDiagnostics<ITwoRequesterProperties>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<ITwoRequesterProperties>(this.requester.Object));
        }
    }
}
