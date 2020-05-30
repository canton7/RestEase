using RestEase.Implementation;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class SanityCheckTests : ImplementationFactoryTestsBase
    {
        public interface IMethodWithoutAttribute
        {
            [Get("foo")]
            Task GetAsync();

            Task SomethingElseAsync();
        }

        public interface IMethodReturningVoid
        {
            [Get("foo")]
            Task GetAsync();

            [Get]
            void ReturnsVoid();
        }

        public interface IMethodReturningString
        {
            [Get("foo")]
            Task GetAsync();

            [Get]
            string ReturnsString();
        }

        public interface IHasMethodParameterWithMultipleAttributes
        {
            [Get]
            Task FooAsync([Query, HttpRequestMessageProperty] string foo);
        }

        public interface IHasEvents
        {
            event EventHandler Foo;
        }

        public interface IHasProperties
        {
            bool SomeProperty { get; }
        }

        public SanityCheckTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ThrowsIfMethodWithoutAttribute()
        {
            this.VerifyDiagnostics<IMethodWithoutAttribute>(
                // (6,18): Error REST018: Method does not have a suitable [Get] / [Post] / etc attribute
                // SomethingElseAsync
                Diagnostic(DiagnosticCode.MethodMustHaveRequestAttribute, "SomethingElseAsync").WithLocation(6, 18)
            );
        }

        [Fact]
        public void ThrowsIfMethodReturningVoid()
        {
            this.VerifyDiagnostics<IMethodReturningVoid>(
                // (7,18): Error REST019: Method must have a return type of Task or Task<T>
                // ReturnsVoid
                Diagnostic(DiagnosticCode.MethodMustHaveValidReturnType, "ReturnsVoid").WithLocation(7, 18)
            );
        }

        [Fact]
        public void ThrowsIfMethodReturningString()
        {
            // Ideally we would test every object that isn't a Task<T>, but that's somewhat impossible...
            this.VerifyDiagnostics<IMethodReturningString>(
                // (7,20): Error REST019: Method must have a return type of Task or Task<T>
                // ReturnsString
                Diagnostic(DiagnosticCode.MethodMustHaveValidReturnType, "ReturnsString").WithLocation(7, 20)
            );
        }

        [Fact]
        public void ThrowsIfMethodWithoutAttributes()
        {
            this.VerifyDiagnostics<IHasMethodParameterWithMultipleAttributes>(
                // (4,27): Error REST025: Method parameter 'foo' has no attributes: it must have at least one
                // [Query, HttpRequestMessageProperty] string foo
                Diagnostic(DiagnosticCode.ParameterMustHaveZeroOrOneAttributes, "[Query, HttpRequestMessageProperty] string foo")
                    .WithLocation(4, 27)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceHasEvents()
        {
            this.VerifyDiagnostics<IHasEvents>(
                // (3,32): Error REST015: Intarface must not have any events
                // Foo
                Diagnostic(DiagnosticCode.EventsNotAllowed, "Foo").WithLocation(3, 32)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceHasProperties()
        {
            this.VerifyDiagnostics<IHasProperties>(
                // (3,18): Error REST020: Property must have exactly one attribute
                // SomeProperty
                Diagnostic(DiagnosticCode.PropertyMustHaveOneAttribute, "SomeProperty").WithLocation(3, 18)
            );
        }

#if !SOURCE_GENERATOR
        [Fact]
        public void ThrowsSameExceptionIfRequestedTwice()
        {
            var e1 = Assert.Throws<ImplementationCreationException>(() => this.CreateImplementation<IHasProperties>());
            var e2 = Assert.Throws<ImplementationCreationException>(() => this.CreateImplementation<IHasProperties>());
            Assert.Same(e1, e2);
        }
#endif
    }
}
