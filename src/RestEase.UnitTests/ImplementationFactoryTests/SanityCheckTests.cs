using RestEase.Implementation;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    internal interface IInternalInterface
    {
        [Get]
        Task FooAsync();
    }

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

        public interface IHasEvents
        {
            event EventHandler Foo;
        }

        public interface IHasProperties
        {
            bool SomeProperty { get; }
        }

        public interface IHasRef
        {
            [Get]
            Task FooAsync(ref int foo);
        }

        public interface IHasOut
        {
            [Get]
            Task FooAsync(out int foo);
        }

        public interface IHasIn
        {
            [Get]
            Task FooAsync(in int foo);
        }

        private interface IPrivateInterface
        {
            [Get]
            Task FooAsync();
        }

#pragma warning disable IDE0040 // Add accessibility modifiers
        interface IImplicitPrivateInterface
#pragma warning restore IDE0040 // Add accessibility modifiers
        {
            [Get]
            Task FooAsync();
        }
        internal interface INestedInternalInterface
        {
            [Get]
            Task FooAsync();
        }

        private class PrivateClass
        {
            public interface IPublicInterfaceInPrivateClass
            {
                [Get]
                Task FooAsync();
            }
        }

        public interface IHasMultipleRequestAttributes
        {
            [Get, Post]
            Task FooAsync();
        }

        public SanityCheckTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ThrowsIfMethodWithoutAttribute()
        {
            VerifyDiagnostics<IMethodWithoutAttribute>(
                // (6,18): Error REST018: Method does not have a suitable [Get] / [Post] / etc attribute
                // SomethingElseAsync
                Diagnostic(DiagnosticCode.MethodMustHaveRequestAttribute, "SomethingElseAsync").WithLocation(6, 18)
            );
        }

        [Fact]
        public void ThrowsIfMethodReturningVoid()
        {
            VerifyDiagnostics<IMethodReturningVoid>(
                // (7,18): Error REST019: Method must have a return type of Task or Task<T>
                // ReturnsVoid
                Diagnostic(DiagnosticCode.MethodMustHaveValidReturnType, "ReturnsVoid").WithLocation(7, 18)
            );
        }

        [Fact]
        public void ThrowsIfMethodReturningString()
        {
            // Ideally we would test every object that isn't a Task<T>, but that's somewhat impossible...
            VerifyDiagnostics<IMethodReturningString>(
                // (7,20): Error REST019: Method must have a return type of Task or Task<T>
                // ReturnsString
                Diagnostic(DiagnosticCode.MethodMustHaveValidReturnType, "ReturnsString").WithLocation(7, 20)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceHasEvents()
        {
            VerifyDiagnostics<IHasEvents>(
                // (3,32): Error REST015: Interface must not have any events
                // Foo
                Diagnostic(DiagnosticCode.EventsNotAllowed, "Foo").WithLocation(3, 32)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceHasProperties()
        {
            VerifyDiagnostics<IHasProperties>(
                // (3,18): Error REST020: Property must have exactly one attribute
                // SomeProperty
                Diagnostic(DiagnosticCode.PropertyMustHaveOneAttribute, "SomeProperty").WithLocation(3, 18)
            );
        }

        [Fact]
        public void ThrowsIfRefInOrOutParameters()
        {
            VerifyDiagnostics<IHasRef>(
                // (4,27): Error REST030: Method parameter 'foo' must not be ref, in or out
                // ref int foo
                Diagnostic(DiagnosticCode.ParameterMustNotBeByRef, "ref int foo").WithLocation(4, 27)
            );

            VerifyDiagnostics<IHasIn>(
                // (4,27): Error REST030: Method parameter 'foo' must not be ref, in or out
                // in int foo
                Diagnostic(DiagnosticCode.ParameterMustNotBeByRef, "in int foo").WithLocation(4, 27)
            );

            VerifyDiagnostics<IHasOut>(
                // (4,27): Error REST030: Method parameter 'foo' must not be ref, in or out
                // out int foo
                Diagnostic(DiagnosticCode.ParameterMustNotBeByRef, "out int foo").WithLocation(4, 27)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceIsPrivate()
        {
            VerifyDiagnostics<IPrivateInterface>(
                // (1,27): Error REST031: Type 'IPrivateInterface' must be public or internal
                // IPrivateInterface
                Diagnostic(DiagnosticCode.InterfaceTypeMustBeAccessible, "IPrivateInterface").WithLocation(1, 27)
            );
        }

        [Fact]
        public void ThrowsIfInterfaceIsImplicitPrivate()
        {
            VerifyDiagnostics<IImplicitPrivateInterface>(
                // (1,19): Error REST031: Type 'IImplicitPrivateInterface' must be public or internal
                // IImplicitPrivateInterface
                Diagnostic(DiagnosticCode.InterfaceTypeMustBeAccessible, "IImplicitPrivateInterface").WithLocation(1, 19)
            );
        }

        [Fact]
        public void ThrowsIfPublicInterfaceInPrivateClass()
        {
            VerifyDiagnostics<PrivateClass.IPublicInterfaceInPrivateClass>(
                // (1,30): Error REST031: Type 'IPublicInterfaceInPrivateClass' must be public or internal
                // IPublicInterfaceInPrivateClass
                Diagnostic(DiagnosticCode.InterfaceTypeMustBeAccessible, "IPublicInterfaceInPrivateClass").WithLocation(1, 30)
            );
        }

        [Fact]
        public void HandlesInternalInterface()
        {
            VerifyDiagnostics<IInternalInterface>(
#if !SOURCE_GENERATOR
                Diagnostic(DiagnosticCode.InterfaceTypeMustBeAccessible, null)
#endif
            );
        }

        [Fact]
        public void HandlesNestedInternalInterface()
        {
            VerifyDiagnostics<INestedInternalInterface>(
#if !SOURCE_GENERATOR
                Diagnostic(DiagnosticCode.InterfaceTypeMustBeAccessible, null)
#endif
            );
        }

        [Fact]
        public void ThrowsIfMultipleRequestAttributes()
        {
            VerifyDiagnostics<IHasMultipleRequestAttributes>(
                // (3,14): Error REST039: Method must only have a single request-related attribute, found (Get, Post)
                // Get
                Diagnostic(DiagnosticCode.MethodMustHaveOneRequestAttribute, @"Get").WithLocation(3, 14).WithLocation(3, 19)
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
