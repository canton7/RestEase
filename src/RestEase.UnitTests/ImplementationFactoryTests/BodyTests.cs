using RestEase.Implementation;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class BodyTests : ImplementationFactoryTestsBase
    {
        public interface IHasTwoBodies
        {
            [Get("foo")]
            Task FooAsync([Body] string body1, [Body] string body2);
        }

        public interface IHasBody
        {
            [Get("foo")]
            Task SerializedAsync([Body(BodySerializationMethod.Serialized)] object serialized);

            [Get("bar")]
            Task UrlEncodedAsync([Body(BodySerializationMethod.UrlEncoded)] object serialized);

            [Get("bar")]
            Task ValueTypeAsync([Body(BodySerializationMethod.UrlEncoded)] int serialized);
        }

        [SerializationMethods(Body = BodySerializationMethod.UrlEncoded)]
        public interface IHasNonOverriddenBodySerializationMethod
        {
            [Get("foo")]
            Task FooAsync([Body] string foo);
        }

        [SerializationMethods(Body = BodySerializationMethod.UrlEncoded)]
        public interface IMethodSerializationAttributeOverridesInterface
        {
            [Get("foo")]
            [SerializationMethods(Body = BodySerializationMethod.Serialized)]
            Task FooAsync([Body] string foo);
        }

        [SerializationMethods(Body = BodySerializationMethod.UrlEncoded)]
        public interface IHasOverriddenBodySerializationMethod
        {
            [Get("foo")]
            Task FooAsync([Body(BodySerializationMethod.Serialized)] string foo);
        }

        public BodyTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ThrowsIfTwoBodies()
        {
            VerifyDiagnostics<IHasTwoBodies>(
                // (4,27): Error REST007: Found more than one parameter with a [Body] attribute
                // [Body] string body1
                Diagnostic(DiagnosticCode.MultipleBodyParameters, "[Body] string body1").WithLocation(4, 27).WithLocation(4, 48)
            );
        }

        [Fact]
        public void BodyWithSerializedClassAsExpected()
        {
            object body = new();
            var requestInfo = this.Request<IHasBody>(x => x.SerializedAsync(body));

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void BodyWithUrlEncodedCallsAsExpected()
        {
            object body = new();
            var requestInfo = this.Request<IHasBody>(x => x.UrlEncodedAsync(body));

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void BodyWithValueTypeCallsAsExpected()
        {
            // Tests that the value is boxed properly
            var requestInfo = this.Request<IHasBody>(x => x.ValueTypeAsync(3));

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(3, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void DefaultBodySerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var requestInfo = this.Request<IHasNonOverriddenBodySerializationMethod>(x => x.FooAsync("yay"));

            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var requestInfo = this.Request<IHasOverriddenBodySerializationMethod>(x => x.FooAsync("yay"));

            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
        }

        [Fact]
        public void MethodSerializationMethodOverridesInterface()
        {
            var requestInfo = this.Request<IMethodSerializationAttributeOverridesInterface>(x => x.FooAsync("yay"));

            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
        }
    }
}
