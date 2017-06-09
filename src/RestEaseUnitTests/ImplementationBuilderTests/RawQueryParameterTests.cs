using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class RawQueryParameterTests
    {
        public class HasToString
        {
            public override string ToString()
            {
                return "HasToString";
            }
        }

        public interface ISimpleRawQueryString
        {
            [Get]
            Task FooAsync([RawQueryString] string rawQueryString);
        }

        public interface ITwoRawQueryStrings
        {
            [Get]
            Task FooAsync([RawQueryString] string one, [RawQueryString] string two);
        }

        public interface ICustomRawQueryString
        {
            [Get]
            Task FooAsync([RawQueryString] HasToString value);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

        [Fact]
        public void AddsRawQueryParam()
        {
            var implementation = this.builder.CreateImplementation<ISimpleRawQueryString>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("&test=test2");

            Assert.NotNull(requestInfo.RawQueryParameter);
            Assert.Equal("&test=test2", requestInfo.RawQueryParameter.SerializeToString());
        }

        [Fact]
        public void ThrowsIfTwoRawQueryParamsSpecified()
        {
            Assert.Throws<ImplementationCreationException>(() => this.builder.CreateImplementation<ITwoRawQueryStrings>(this.requester.Object));
        }

        [Fact]
        public void CallsToStringOnParam()
        {
            var implementation = this.builder.CreateImplementation<ICustomRawQueryString>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new HasToString());

            Assert.NotNull(requestInfo.RawQueryParameter);
            Assert.Equal("HasToString", requestInfo.RawQueryParameter.SerializeToString());
        }
    }
}
