using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
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

            void ReturnsVoid();
        }

        public interface IMethodReturningString
        {
            [Get("foo")]
            Task GetAsync();

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

        public SanityCheckTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ThrowsIfMethodWithoutAttribute()
        {
            this.VerifyDiagnostics<IMethodWithoutAttribute>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodWithoutAttribute>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningVoid()
        {
            this.VerifyDiagnostics<IMethodReturningVoid>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodReturningVoid>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningString()
        {
            // Ideally we would test every object that isn't a Task<T>, but that's somewhat impossible...
            this.VerifyDiagnostics<IMethodReturningString>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodReturningString>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfInterfaceHasEvents()
        {
            this.VerifyDiagnostics<IHasEvents>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasEvents>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfInterfaceHasProperties()
        {
            this.VerifyDiagnostics<IHasProperties>();
            //Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasProperties>(this.requester.Object));
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
