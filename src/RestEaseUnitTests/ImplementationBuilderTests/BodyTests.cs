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
    public class BodyTests
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


        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void ThrowsIfTwoBodies()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<IHasTwoBodies>(this.requester.Object));
        }

        [Fact]
        public void BodyWithSerializedClassAsExpected()
        {
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var body = new object();
            implementation.SerializedAsync(body);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void BodyWithUrlEncodedCallsAsExpected()
        {
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var body = new object();
            implementation.UrlEncodedAsync(body);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(body, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void BodyWithValueTypeCallsAsExpected()
        {
            // Tests that the value is boxed properly
            var implementation = this.builder.CreateImplementation<IHasBody>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.ValueTypeAsync(3);

            Assert.NotNull(requestInfo.BodyParameterInfo);
            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
            Assert.Equal(3, requestInfo.BodyParameterInfo.ObjectValue);
        }

        [Fact]
        public void DefaultBodySerializationMethodIsSpecifiedBySerializationMethodsHeader()
        {
            var implementation = this.builder.CreateImplementation<IHasNonOverriddenBodySerializationMethod>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("yay");

            Assert.Equal(BodySerializationMethod.UrlEncoded, requestInfo.BodyParameterInfo.SerializationMethod);
        }

        [Fact]
        public void DefaultQuerySerializationMethodCanBeOverridden()
        {
            var implementation = this.builder.CreateImplementation<IHasOverriddenBodySerializationMethod>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("yay");

            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
        }

        [Fact]
        public void MethodSerializationMethodOverridesInterface()
        {
            var implementation = this.builder.CreateImplementation<IMethodSerializationAttributeOverridesInterface>(this.requester.Object);

            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("yay");

            Assert.Equal(BodySerializationMethod.Serialized, requestInfo.BodyParameterInfo.SerializationMethod);
        }
    }
}
