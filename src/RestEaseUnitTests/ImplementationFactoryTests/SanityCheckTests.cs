using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class SanityCheckTests
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

        public interface IRequesterPropertyWithoutGetter
        {
            IRequester Requester { set; }
        }

        public interface IRequesterPropertyWithSetter
        {
            IRequester Requester { get; set; }
        }

        public interface ITwoRequesterProperties
        {
            IRequester Requester1 { get; }
            IRequester Requester2 { get; }
        }

        public interface IGenericApi<T>
        {
            [Get("foo")]
            Task<T> FooAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void ThrowsIfMethodWithoutAttribute()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodWithoutAttribute>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningVoid()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodReturningVoid>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodReturningString()
        {
            // Ideally we would test every object that isn't a Task<T>, but that's somewhat impossible...
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IMethodReturningString>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfInterfaceHasEvents()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasEvents>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfInterfaceHasProperties()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IHasProperties>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfRequesterHasNoGetter()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IRequesterPropertyWithoutGetter>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfRequesterHasSetter()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<IRequesterPropertyWithSetter>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfTwoRequesters()
        {
            Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<ITwoRequesterProperties>(this.requester.Object));
        }

        [Fact]
        public void AllowsGenericApis()
        {
            var implementation = this.factory.CreateImplementation<IGenericApi<int>>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestAsync<int>(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(3));

            int result = implementation.FooAsync().Result;

            Assert.Equal(3, result);
        }

        [Fact]
        public void ThrowsSameExceptionIfRequestedTwice()
        {
            var e1 = Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<ITwoRequesterProperties>(this.requester.Object));
            var e2 = Assert.Throws<ImplementationCreationException>(() => this.factory.CreateImplementation<ITwoRequesterProperties>(this.requester.Object));
            Assert.Same(e1, e2);
        }
    }
}
