using RestEase.Implementation;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
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

        public interface IHasMultiplePropertiesForKey
        {
            [HttpRequestMessageProperty]
            string Foo { get; set; }

            [HttpRequestMessageProperty("Foo")]
            string Baz { get; set; }
        }

        public interface IHasMultipleParametersForKey
        {
            [Get]
            Task FooAsync([HttpRequestMessageProperty] string a, [HttpRequestMessageProperty("a")] string b);
        }

        public interface IHasDuplicatePropertyAndParameterForKey
        {
            [HttpRequestMessageProperty]
            string Bar { get; set; }

            [Get]
            Task BarAsync([HttpRequestMessageProperty("Bar")] string bar);
        }

        public HttpRequestMessagePropertyTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesPathProperty()
        {
            object propertyValue = new();
            object parameterValue = new();
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

        [Fact]
        public void ThrowsIfMultiplePropertiesForKey()
        {
            VerifyDiagnostics<IHasMultiplePropertiesForKey>(
                // (3,13): Error REST022: Multiple properties found for HttpRequestMessageProperty key 'Foo'
                // [HttpRequestMessageProperty]
                //         string Foo { get; set; }
                Diagnostic(DiagnosticCode.MultipleHttpRequestMessagePropertiesForKey, @"[HttpRequestMessageProperty]
            string Foo { get; set; }").WithLocation(3, 13).WithLocation(6, 13)
            );
        }

        [Fact]
        public void ThrowsIfMultipleParametersForKey()
        {
            VerifyDiagnostics<IHasMultipleParametersForKey>(
                 // (4,27): Error REST024: Multiple parameters found for HttpRequestMessageProperty key 'a'
                 // [HttpRequestMessageProperty] string a
                 Diagnostic(DiagnosticCode.MultipleHttpRequestMessageParametersForKey, "[HttpRequestMessageProperty] string a")
                     .WithLocation(4, 27).WithLocation(4, 66)
            );
        }

        [Fact]
        public void ThrowsIfDuplicateParameterAndPropertyForKey()
        {
            VerifyDiagnostics<IHasDuplicatePropertyAndParameterForKey>(
                // (7,27): Error REST023: Method parameter has the same HttpRequestMessageProperty key 'Bar' as property 'Bar'
                // [HttpRequestMessageProperty("Bar")] string bar
                Diagnostic(DiagnosticCode.HttpRequestMessageParamDuplicatesPropertyForKey, @"[HttpRequestMessageProperty(""Bar"")] string bar")
                    .WithLocation(7, 27)
            );
        }
    }
}
