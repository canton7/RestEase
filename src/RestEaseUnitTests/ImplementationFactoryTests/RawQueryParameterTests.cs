using System;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;
using System.Globalization;
using System.Linq;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class RawQueryParameterTests
    {
        public class HasToString : IFormattable
        {
            public IFormatProvider LastFormatProvider { get; set; }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                this.LastFormatProvider = formatProvider;
                3.ToString(formatProvider); // Just call this
                return "HasToString";
            }

            public override string ToString() => "HasToString";
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
        private readonly EmitImplementationFactory factory = EmitImplementationFactory.Instance;

        [Fact]
        public void AddsRawQueryParam()
        {
            var implementation = this.factory.CreateImplementation<ISimpleRawQueryString>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("test=test2");

            Assert.NotNull(requestInfo.RawQueryParameters);
            Assert.Single(requestInfo.RawQueryParameters);
            Assert.Equal("test=test2", requestInfo.RawQueryParameters.First().SerializeToString(null));
        }

        [Fact]
        public void AddsTwoRawQueryParam()
        {
            var implementation = this.factory.CreateImplementation<ITwoRawQueryStrings>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("test=test2", "test3=test4");

            Assert.NotNull(requestInfo.RawQueryParameters);
            var rawQueryParameters = requestInfo.RawQueryParameters.ToList();
            Assert.Equal(2, rawQueryParameters.Count);
            Assert.Equal("test=test2", rawQueryParameters[0].SerializeToString(null));
            Assert.Equal("test3=test4", rawQueryParameters[1].SerializeToString(null));
        }

        [Fact]
        public void CallsToStringOnParam()
        {
            var implementation = this.factory.CreateImplementation<ICustomRawQueryString>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new HasToString());

            Assert.NotNull(requestInfo.RawQueryParameters);
            var rawQueryParameters = requestInfo.RawQueryParameters.ToList();
            Assert.Single(rawQueryParameters);
            Assert.Equal("HasToString", rawQueryParameters[0].SerializeToString(null));
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var implementation = this.factory.CreateImplementation<ICustomRawQueryString>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            var hasToString = new HasToString();
            implementation.FooAsync(hasToString);

            var formatProvider = new Mock<IFormatProvider>();

            requestInfo.RawQueryParameters.First().SerializeToString(formatProvider.Object);

            Assert.Equal(formatProvider.Object, hasToString.LastFormatProvider);
            //formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }
    }
}
