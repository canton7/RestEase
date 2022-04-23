using RestEase.Implementation;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
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

        public interface IHasRequesterPropertyWithAttribute
        {
            [Header("Foo")]
            [HttpRequestMessageProperty]
            IRequester Requester { get; }
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
            VerifyDiagnostics<IHasSet>(
                // (3,24): Error REST016: Property must have a getter but not a setter
                // Requester
                Diagnostic(DiagnosticCode.PropertyMustBeReadOnly, "Requester").WithLocation(3, 24)
            );
        }

        [Fact]
        public void ThrowsIfHasNoGet()
        {
            VerifyDiagnostics<IHasNoGet>(
                // (3,24): Error REST016: Property must have a getter but not a setter
                // Requester
                Diagnostic(DiagnosticCode.PropertyMustBeReadOnly, "Requester").WithLocation(3, 24)
            );
        }

        [Fact]
        public void ThrowsIfTwoRequesters()
        {
            VerifyDiagnostics<ITwoRequesterProperties>(
                // (4,24): Error REST017: There must not be more than one property of type IRequester
                // Requester2
                Diagnostic(DiagnosticCode.MultipleRequesterProperties, "Requester2").WithLocation(4, 24)
            );
        }

        [Fact]
        public void ThrowsIfHasAttributes()
        {
            VerifyDiagnostics<IHasRequesterPropertyWithAttribute>(
                // (3,14): Error REST021: IRequester property must not have any attribtues
                // Header("Foo")
                Diagnostic(DiagnosticCode.RequesterPropertyMustHaveZeroAttributes, @"Header(""Foo"")")
                    .WithLocation(3, 14).WithLocation(4, 14)
            );
        }
    }
}
