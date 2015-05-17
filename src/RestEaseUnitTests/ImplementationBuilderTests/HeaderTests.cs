using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class HeaderTests
    {
        [Header("Class Header 1", "Yes")]
        [Header("Class Header 2", "Yes")]
        public interface IHasClassHeaders
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasMethodHeaders
        {
            [Get("foo")]
            [Header("Method Header 1", "Yes")]
            [Header("Method Header 2", "Yes")]
            Task FooAsync();
        }

        public interface IHasParamHeaders
        {
            [Get("foo")]
            Task FooAsync([Header("Param Header 1")] string foo, [Header("Param Header 2")] string bar);
        }

        public interface IHasParamHeaderWithValue
        {
            [Get("foo")]
            Task FooAsync([Header("Param Header", "ShouldNotBeSet")] string foo);
        }

        [Header("Foo")]
        public interface IHasClassHeaderWithoutValue
        {
            [Get("foo")]
            Task FooAsync();
        }

        [Header("Foo: Bar", "Bar")]
        public interface IHasClassHeaderWithColon
        {
            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasMethodHeaderWithColon
        {
            [Get("foo")]
            [Header("Foo: Bar", "Baz")]
            Task FooAsync();
        }

        public interface IHasHeaderParamWithColon
        {
            [Get("foo")]
            Task FooAsync([Header("Foo: Bar")] string foo);
        }

        public interface IHasHeaderParamWithValue
        {
            [Get("foo")]
            Task FooAsync([Header("Foo", "Bar")] string foo);
        }
        
        public interface IHasPropertyHeaderWithValue
        {
            [Header("Name", "Value")]
            string Header { get; set; }
        }

        public interface IHasPropertyHeaderWithColon
        {
            [Header("Name: Value")]
            string Header { get; set; }
        }

        public interface IHasPropertyHeaderWithGetterOnly
        {
            [Header("Name")]
            string Header { get; }
        }

        public interface IHasPropertyHeaderWithSetterOnly
        {
            [Header("Name")]
            string Header { set; }
        }

        public interface IHasPropertyHeader
        {
            [Header("X-API-Key")]
            string ApiKey { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void HandlesClassHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasClassHeaders>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();
            var expected = new[]
            {
                new KeyValuePair<string, string>("Class Header 1", "Yes"),
                new KeyValuePair<string, string>("Class Header 2", "Yes"),
            };

            Assert.Equal(expected, requestInfo.ClassHeaders.OrderBy(x => x.Key));
        }

        [Fact]
        public void HandlesMethodHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasMethodHeaders>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();
            var expected = new[]
            {
                new KeyValuePair<string, string>("Method Header 1", "Yes"),
                new KeyValuePair<string, string>("Method Header 2", "Yes"),
            };

            Assert.Equal(expected, requestInfo.MethodHeaders.OrderBy(x => x.Key));
        }

        [Fact]
        public void HandlesParamHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasParamHeaders>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("value 1", "value 2");

            Assert.Equal(2, requestInfo.HeaderParams.Count);

            Assert.Equal("Param Header 1", requestInfo.HeaderParams[0].Key);
            Assert.Equal("value 1", requestInfo.HeaderParams[0].Value);

            Assert.Equal("Param Header 2", requestInfo.HeaderParams[1].Key);
            Assert.Equal("value 2", requestInfo.HeaderParams[1].Value);
        }

        [Fact]
        public void ThrowsIfParamHeaderHasValue()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasParamHeaderWithValue>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfClassHeaderDoesNotHaveValue()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasClassHeaderWithoutValue>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfClassHeaderHasColon()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasClassHeaderWithColon>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfMethodHeaderHasColon()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasMethodHeaderWithColon>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfHeaderParamHasColon()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasHeaderParamWithColon>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfHeaderParamHasValue()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasHeaderParamWithValue>(this.requester.Object)); ;
        }

        [Fact]
        public void ThrowsIfPropertyHeaderHasValue()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPropertyHeaderWithValue>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPropertyHeaderHasColon()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPropertyHeaderWithColon>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPropertyHeaderOnlyHasGetter()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPropertyHeaderWithGetterOnly>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPropertyHeaderOnlyHasSetter()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPropertyHeaderWithSetterOnly>(this.requester.Object));
        }

        [Fact]
        public void HandlesPropertyHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasPropertyHeader>(this.requester.Object);
            implementation.ApiKey = "Foo Bar";
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            Assert.Equal(1, requestInfo.PropertyHeaders.Count);

            Assert.Equal("X-API-Key", requestInfo.PropertyHeaders[0].Key);
            Assert.Equal("Foo Bar", requestInfo.PropertyHeaders[0].Value);
        }
    }
}
