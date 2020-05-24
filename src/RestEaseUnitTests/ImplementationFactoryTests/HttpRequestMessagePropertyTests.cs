using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class HttpRequestMessagePropertyTests : ImplementationFactoryTestsBase
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

        public HttpRequestMessagePropertyTests(ITestOutputHelper output) : base(output) { }

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
    }
}
