using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        public interface IHasParamHeaderOfNonStringType
        {
            [Get("foo")]
            Task FooAsync([Header("Param Header")] int foo);
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
            int Header { get; set; }
        }

        public interface IHasNullablePropertyHeaderWithValue
        {
            [Header("Name", "Value")]
            int? Header { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasObjectPropertyHeaderWithValue
        {
            [Header("Name", "Value")]
            string Header { get; set; }

            [Get("foo")]
            Task FooAsync();
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
            [Header("X-API-Key", "IgnoredDefault")]
            string ApiKey { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasAuthorizationHeader
        {
            [Header("Authorization")]
            AuthenticationHeaderValue Authorization { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasNullHeaderValues
        {
            [Header("foo", null)]
            string Foo { get; set; }

            [Header("bar", null)]
            [Get("Foo")]
            Task FooAsync([Header("baz")] string header);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

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

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Equal(2, headerParams.Count);

            Assert.Equal("Param Header 1", headerParams[0].Key);
            Assert.Equal("value 1", headerParams[0].Value);

            Assert.Equal("Param Header 2", headerParams[1].Key);
            Assert.Equal("value 2", headerParams[1].Value);
        }

        [Fact]
        public void HandlesNullParamHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasParamHeaders>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("value 1", null);

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Equal(2, headerParams.Count);

            Assert.Equal("Param Header 2", headerParams[1].Key);
            Assert.Null(headerParams[1].Value);
        }

        [Fact]
        public void HandlesNonStringParamHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasParamHeaderOfNonStringType>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(3);

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Single(headerParams);

            Assert.Equal("Param Header", headerParams[0].Key);
            Assert.Equal("3", headerParams[0].Value);
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
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasHeaderParamWithValue>(this.requester.Object));
        }

        [Fact]
        public void ThrowsIfPropertyHeaderIsValueTypeAndHasValue()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasPropertyHeaderWithValue>(this.requester.Object));
        }

        [Fact]
        public void UsesSpecifiedDefaultForNullableProperty()
        {
            var implementation = this.builder.CreateImplementation<IHasNullablePropertyHeaderWithValue>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);
            Assert.Equal("Name", propertyHeaders[0].Key);
            Assert.Equal("Value", propertyHeaders[0].Value);
        }

        [Fact]
        public void UsesSpecifiedDefaultForObjectProperty()
        {
            var implementation = this.builder.CreateImplementation<IHasObjectPropertyHeaderWithValue>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);
            Assert.Equal("Name", propertyHeaders[0].Key);
            Assert.Equal("Value", propertyHeaders[0].Value);
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

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);

            Assert.Equal("X-API-Key", propertyHeaders[0].Key);
            Assert.Equal("Foo Bar", propertyHeaders[0].Value);
        }

        [Fact]
        public void PropertyHeadersBehaveAsReadHeaders()
        {
            var implementation = this.builder.CreateImplementation<IHasPropertyHeader>(this.requester.Object);

            Assert.Null(implementation.ApiKey);

            implementation.ApiKey = "test";
            Assert.Equal("test", implementation.ApiKey);

            implementation.ApiKey = null;
            Assert.Null(implementation.ApiKey);
        }

        [Fact]
        public void SpecifyingAuthorizationHeaderWorksAsExpected()
        {
            var implementation = this.builder.CreateImplementation<IHasAuthorizationHeader>(this.requester.Object);
            // Values from http://en.wikipedia.org/wiki/Basic_access_authentication#Client_side
            var value = Convert.ToBase64String(Encoding.ASCII.GetBytes("Aladdin:open sesame"));
            implementation.Authorization = new AuthenticationHeaderValue("Basic", value);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync();

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);

            Assert.Equal("Authorization", propertyHeaders[0].Key);
            Assert.Equal("Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==", propertyHeaders[0].Value);
        }

        [Fact]
        public void HandlesNullHeaderValues()
        {
            this.builder.CreateImplementation<IHasNullHeaderValues>(this.requester.Object);
        }
    }
}
