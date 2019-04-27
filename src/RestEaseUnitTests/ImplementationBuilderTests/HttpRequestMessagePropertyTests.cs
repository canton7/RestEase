using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class HttpRequestMessagePropertyTests
    {
        public interface IRequestProperties
        {
            [RequestProperty]
            object PropertyFoo { get; set; }

            [RequestProperty("bar-property")]
            object P2 { get; set; }

            [Get("foo")]
            Task FooAsync([RequestProperty] object foo, [RequestProperty("bar-parameter")] object p2);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

        [Fact]
        public void HandlesPathProperty()
        {
            var requestProperty1 = new object();
            var requestProperty2 = new object();
            var requestInfo =
                Request<IRequestProperties>(
                    x =>
                    {
                        x.PropertyFoo = "foo";
                        x.P2 = "bar";
                        return x.FooAsync(requestProperty1, requestProperty2);
                    });

            var requestProperties = requestInfo.HttpRequestMessageProperties.ToList();

            Assert.Equal(4, requestProperties.Count);

            var propertyPropertyFoo = requestProperties[0];
            Assert.Equal(nameof(IRequestProperties.PropertyFoo), propertyPropertyFoo.Key);
            Assert.Equal("foo", propertyPropertyFoo.Value);

            var propertyP2 = requestProperties[1];
            Assert.Equal("bar-property", propertyP2.Key);
            Assert.Equal("bar", propertyP2.Value);

            var propertyParam1 = requestProperties[2];
            Assert.Equal("foo", propertyParam1.Key);
            Assert.Equal(requestProperty1, propertyParam1.Value);

            var propertyParam2 = requestProperties[3];
            Assert.Equal("bar-parameter", propertyParam2.Key);
            Assert.Equal(requestProperty2, propertyParam2.Value);
        }

        private IRequestInfo Request<T>(Func<T, Task> selector)
        {
            var implementation = this.builder.CreateImplementation<T>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            selector(implementation);

            return requestInfo;
        }
    }
}
