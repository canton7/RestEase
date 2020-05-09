using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class HttpRequestMessagePropertyTests
    {
        public interface IHttpRequestMessageProperties
        {
            [HttpRequestMessageProperty]
            object PropertyFoo { get; set; }

            [HttpRequestMessageProperty("bar-property")]
            int P2 { get; set; }

            [Get("foo")]
            Task FooAsync([HttpRequestMessageProperty] object foo, [HttpRequestMessageProperty("bar-parameter")] decimal valueType);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationFactory builder = ImplementationFactory.Instance;

        [Fact]
        public void HandlesPathProperty()
        {
            object propertyValue = new object();
            object parameterValue = new object();
            var requestInfo =
                this.Request<IHttpRequestMessageProperties>(
                    x =>
                    {
                        x.PropertyFoo = propertyValue;
                        x.P2 = 1;
                        return x.FooAsync(parameterValue, 123.456m);
                    });

            var httpRequestMessageProperties = requestInfo.HttpRequestMessageProperties.ToList();

            Assert.Equal(4, httpRequestMessageProperties.Count);

            var propertyPropertyFoo = httpRequestMessageProperties[0];
            Assert.Equal(nameof(IHttpRequestMessageProperties.PropertyFoo), propertyPropertyFoo.Key);
            Assert.Equal(propertyValue, propertyPropertyFoo.Value);

            var propertyP2 = httpRequestMessageProperties[1];
            Assert.Equal("bar-property", propertyP2.Key);
            Assert.Equal(1, propertyP2.Value);

            var propertyParam1 = httpRequestMessageProperties[2];
            Assert.Equal("foo", propertyParam1.Key);
            Assert.Equal(parameterValue, propertyParam1.Value);

            var propertyParam2 = httpRequestMessageProperties[3];
            Assert.Equal("bar-parameter", propertyParam2.Key);
            Assert.Equal(123.456m, propertyParam2.Value);
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
