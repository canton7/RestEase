using Moq;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class HeaderTests : ImplementationFactoryTestsBase
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
            Task FooAsync([Header("Param Header 1")] string foo, [Header("Param Header 2")] object bar);
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

        public interface IHasFormattedPathHeader
        {
            [Header("foo", Format = "C")]
            int Foo { get; set; }

            [Get("foo")]
            Task FooAsync();
        }

        public interface IHasFormattedHeaderParam
        {
            [Get("foo")]
            Task FooAsync([Header("Foo", Format = "X2")] int foo);
        }

        public HeaderTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesClassHeaders()
        {
            var requestInfo = this.Request<IHasClassHeaders>(x => x.FooAsync());
            var expected = new[]
            {
                new KeyValuePair<string, string>("Class Header 1", "Yes"),
                new KeyValuePair<string, string>("Class Header 2", "Yes"),
            };

            Assert.NotNull(requestInfo.ClassHeaders);
            Assert.Equal(expected, requestInfo.ClassHeaders.OrderBy(x => x.Key));
        }

        [Fact]
        public void HandlesMethodHeaders()
        {
            var requestInfo = this.Request<IHasMethodHeaders>(x => x.FooAsync());
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
            var requestInfo = this.Request<IHasParamHeaders>(x => x.FooAsync("value 1", "value 2"));

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Equal(2, headerParams.Count);

            var serialized1 = headerParams[0].SerializeToString(null);
            Assert.Equal("Param Header 1", serialized1.Key);
            Assert.Equal("value 1", serialized1.Value);

            var serialized2 = headerParams[1].SerializeToString(null);
            Assert.Equal("Param Header 2", serialized2.Key);
            Assert.Equal("value 2", serialized2.Value);
        }

        [Fact]
        public void HandlesNullParamHeaders()
        {
            var requestInfo = this.Request<IHasParamHeaders>(x => x.FooAsync("value 1", null));

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Equal(2, headerParams.Count);

            var serialized = headerParams[1].SerializeToString(null);
            Assert.Equal("Param Header 2", serialized.Key);
            Assert.Null(serialized.Value);
        }

        [Fact]
        public void HandlesNonStringParamHeaders()
        {
            var requestInfo = this.Request<IHasParamHeaderOfNonStringType>(x => x.FooAsync(3));

            var headerParams = requestInfo.HeaderParams.ToList();

            Assert.Single(headerParams);

            var serialized = headerParams[0].SerializeToString(null);
            Assert.Equal("Param Header", serialized.Key);
            Assert.Equal("3", serialized.Value);
        }

        [Fact]
        public void ThrowsIfParamHeaderHasValue()
        {
            this.VerifyDiagnostics<IHasParamHeaderWithValue>(
                // (4,28): Error REST009: Header attribute must have the form [Header("Param Header")], not [Header("Param Header", "ShouldNotBeSet")]
                // Header("Param Header", "ShouldNotBeSet")
                Diagnostic(DiagnosticCode.HeaderParameterMustNotHaveValue, @"Header(""Param Header"", ""ShouldNotBeSet"")").WithLocation(4, 28)
            );
        }

        [Fact]
        public void ThrowsIfClassHeaderDoesNotHaveValue()
        {
            this.VerifyDiagnostics<IHasClassHeaderWithoutValue>(
                // (1,10): Error REST008: Header on interface must have a value (i.e. be of the form [Header("Foo", "Value Here")])
                // Header("Foo")
                Diagnostic(DiagnosticCode.HeaderOnInterfaceMustHaveValue, @"Header(""Foo"")").WithLocation(1, 10)
            );
        }

        [Fact]
        public void ThrowsIfClassHeaderHasColon()
        {
            this.VerifyDiagnostics<IHasClassHeaderWithColon>(
                // (1,10): Error REST010: Header attribute name 'Foo: Bar' must not contain a colon
                // Header("Foo: Bar", "Bar")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""Foo: Bar"", ""Bar"")").WithLocation(1, 10)
            );
        }

        [Fact]
        public void ThrowsIfMethodHeaderHasColon()
        {
            this.VerifyDiagnostics<IHasMethodHeaderWithColon>(
                // (4,14): Error REST010: Header attribute name 'Foo: Bar' must not contain a colon
                // Header("Foo: Bar", "Baz")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""Foo: Bar"", ""Baz"")").WithLocation(4, 14)
            );
        }

        [Fact]
        public void ThrowsIfHeaderParamHasColon()
        {
            this.VerifyDiagnostics<IHasHeaderParamWithColon>(
                // (4,28): Error REST010: Header attribute name 'Foo: Bar' must not contain a colon
                // Header("Foo: Bar")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""Foo: Bar"")").WithLocation(4, 28)
            );
        }

        [Fact]
        public void ThrowsIfHeaderParamHasValue()
        {
            this.VerifyDiagnostics<IHasHeaderParamWithValue>(
                // (4,28): Error REST009: Header attribute must have the form [Header("Foo")], not [Header("Foo", "Bar")]
                // Header("Foo", "Bar")
                Diagnostic(DiagnosticCode.HeaderParameterMustNotHaveValue, @"Header(""Foo"", ""Bar"")").WithLocation(4, 28)
            );
        }

        [Fact]
        public void ThrowsIfPropertyHeaderIsValueTypeAndHasValue()
        {
            this.VerifyDiagnostics<IHasPropertyHeaderWithValue>(
                // (3,14): Error REST012: [Header("Name", "Value")] on property (i.e. containing a default value) can only be used if the property type is nullable
                // Header("Name", "Value")
                Diagnostic(DiagnosticCode.HeaderPropertyWithValueMustBeNullable, @"Header(""Name"", ""Value"")").WithLocation(3, 14)
            );
        }

        [Fact]
        public void UsesSpecifiedDefaultForNullableProperty()
        {
            var requestInfo = this.Request<IHasNullablePropertyHeaderWithValue>(x => x.FooAsync());

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);
            var serialized = propertyHeaders[0].SerializeToString(null);
            Assert.Equal("Name", serialized.Key);
            Assert.Equal("Value", serialized.Value);
        }

        [Fact]
        public void UsesSpecifiedDefaultForObjectProperty()
        {
            var requestInfo = this.Request<IHasObjectPropertyHeaderWithValue>(x => x.FooAsync());

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);
            var serialized = propertyHeaders[0].SerializeToString(null);
            Assert.Equal("Name", serialized.Key);
            Assert.Equal("Value", serialized.Value);
        }

        [Fact]
        public void ThrowsIfPropertyHeaderHasColon()
        {
            this.VerifyDiagnostics<IHasPropertyHeaderWithColon>(
                // (3,14): Error REST010: Header attribute name 'Name: Value' must not contain a colon
                // Header("Name: Value")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""Name: Value"")").WithLocation(3, 14)
            );
        }

        [Fact]
        public void ThrowsIfPropertyHeaderOnlyHasGetter()
        {
            this.VerifyDiagnostics<IHasPropertyHeaderWithGetterOnly>(
                // (4,20): Error REST011: Property must have a getter and a setter
                // Header
                Diagnostic(DiagnosticCode.PropertyMustBeReadWrite, "Header").WithLocation(4, 20)
            );
        }

        [Fact]
        public void ThrowsIfPropertyHeaderOnlyHasSetter()
        {
            this.VerifyDiagnostics<IHasPropertyHeaderWithSetterOnly>(
                // (4,20): Error REST011: Property must have a getter and a setter
                // Header
                Diagnostic(DiagnosticCode.PropertyMustBeReadWrite, "Header").WithLocation(4, 20)
            );
        }

        [Fact]
        public void HandlesPropertyHeaders()
        {
            var requestInfo = this.Request<IHasPropertyHeader>(x =>
            {
                x.ApiKey = "Foo Bar";
                return x.FooAsync();
            });

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);

            var serialized = propertyHeaders[0].SerializeToString(null);
            Assert.Equal("X-API-Key", serialized.Key);
            Assert.Equal("Foo Bar", serialized.Value);
        }

        [Fact]
        public void PropertyHeadersBehaveAsReadHeaders()
        {
            var implementation = this.CreateImplementation<IHasPropertyHeader>();

            Assert.Null(implementation.ApiKey);

            implementation.ApiKey = "test";
            Assert.Equal("test", implementation.ApiKey);

            implementation.ApiKey = null;
            Assert.Null(implementation.ApiKey);
        }

        [Fact]
        public void SpecifyingAuthorizationHeaderWorksAsExpected()
        {
            // Values from http://en.wikipedia.org/wiki/Basic_access_authentication#Client_side
            string value = Convert.ToBase64String(Encoding.ASCII.GetBytes("Aladdin:open sesame"));
            var requestInfo = this.Request<IHasAuthorizationHeader>(x =>
            {
                x.Authorization = new AuthenticationHeaderValue("Basic", value);
                return x.FooAsync();
            });

            var propertyHeaders = requestInfo.PropertyHeaders.ToList();

            Assert.Single(propertyHeaders);

            var serialized = propertyHeaders[0].SerializeToString(null);
            Assert.Equal("Authorization", serialized.Key);
            Assert.Equal("Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==", serialized.Value);
        }

        [Fact]
        public void HandlesNullHeaderValues()
        {
            this.VerifyDiagnostics<IHasNullHeaderValues>();
        }

        [Fact]
        public void HandlesFormattedPathHeader()
        {
            var requestInfo = this.Request<IHasFormattedPathHeader>(x =>
            {
                x.Foo = 10;
                return x.FooAsync();
            });

            Assert.Single(requestInfo.PropertyHeaders);
            var serialized = requestInfo.PropertyHeaders.First().SerializeToString(CultureInfo.InvariantCulture);
            Assert.Equal("foo", serialized.Key);
            Assert.Equal("¤10.00", serialized.Value);
        }

        [Fact]
        public void HandlesFormattedHeaderParam()
        {
            var requestInfo = this.Request<IHasFormattedHeaderParam>(x => x.FooAsync(12));

            Assert.Single(requestInfo.HeaderParams);
            var serialized = requestInfo.HeaderParams.First().SerializeToString(null);
            Assert.Equal("Foo", serialized.Key);
            Assert.Equal("0C", serialized.Value);
        }

        [Fact]
        public void FormattedPathHeaderUsesGivenFormatProvider()
        {
            var formatProvider = new Mock<IFormatProvider>();
            var requestInfo = this.Request<IHasFormattedPathHeader>(x => x.FooAsync());

            Assert.Single(requestInfo.PropertyHeaders);
            var serialized = requestInfo.PropertyHeaders.First().SerializeToString(formatProvider.Object);

            formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }
    }
}
