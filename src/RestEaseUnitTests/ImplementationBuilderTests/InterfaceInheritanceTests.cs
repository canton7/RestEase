using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class InterfaceInheritanceTests
    {
        public interface IPropertyChild
        {
            [Header("X-Api-Token")]
            string ApiToken { get; set; }
        }

        public interface IPropertyParent : IPropertyChild
        {
            [Header("X-Api-Username")]
            string ApiUsername { get; set; }
        }

        public interface IMethodChild
        {
            [Get("/foo")]
            Task<string> GetFoo();
        }

        public interface IMethodParent : IMethodChild
        {
            [Get("/bar")]
            Task<string> GetBar();
        }

        [Header("X-Foo", "Bar")]
        public interface IChildWithInterfaceHeader
        {
        }

        [Header("X-Foo", "Baz")]
        public interface IParentWithInterfaceHeader : IChildWithInterfaceHeader
        {
            [Get("/foo")]
            Task FooAsync();
        }

        [AllowAnyStatusCode]
        public interface IChildWithAllowAnyStatusCode
        {
        }

        public interface IParentWithAllowAnyStatusCode : IChildWithAllowAnyStatusCode
        {
        }

        public interface IChildWithInvalidHeaderProperty
        {
            [Header("X-Foo:")]
            string Foo { get; set; }
        }

        public interface IParentWithInvalidHeaderProperty : IChildWithInvalidHeaderProperty
        {
        }

        public interface IChildWithEvent
        {
            event EventHandler Foo;
        }

        public interface IParentWithEvent : IChildWithEvent
        {
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void ImplementsPropertiesFromChild()
        {
            // Does not throw
            this.builder.CreateImplementation<IPropertyParent>(this.requester.Object);
        }

        [Fact]
        public void ImplementsMethodsFromChild()
        {
            // Does not throw
            this.builder.CreateImplementation<IMethodParent>(this.requester.Object);
        }

        [Fact]
        public void CombinesHeadersFromChildInterfaces()
        {
            var impl = this.builder.CreateImplementation<IParentWithInterfaceHeader>(this.requester.Object);

            IRequestInfo requestInfo = null;
            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback<IRequestInfo>(r => requestInfo = r)
                .Returns(Task.FromResult(false));
            impl.FooAsync();

            Assert.NotNull(requestInfo);

            Assert.Equal("X-Foo", requestInfo.ClassHeaders[0].Key);
            Assert.Equal("Baz", requestInfo.ClassHeaders[0].Value);

            Assert.Equal("X-Foo", requestInfo.ClassHeaders[1].Key);
            Assert.Equal("Bar", requestInfo.ClassHeaders[1].Value);
        }

        [Fact]
        public void DoesNotAllowAllowAnyStatusCodeOnChildInterfaces()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IParentWithAllowAnyStatusCode>(this.requester.Object));
        }

        [Fact]
        public void ValidatesHeadersOnChildProperties()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IParentWithInvalidHeaderProperty>(this.requester.Object));
        }

        [Fact]
        public void ValidatesEventsInChildInterfaces()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IParentWithEvent>(this.requester.Object));
        }
    }
}
