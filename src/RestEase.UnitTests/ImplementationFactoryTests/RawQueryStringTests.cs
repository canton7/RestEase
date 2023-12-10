using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class RawQueryStringTests : ImplementationFactoryTestsBase
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

        public RawQueryStringTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void AddsRawQueryParam()
        {
            var requestInfo = this.Request<ISimpleRawQueryString>(x => x.FooAsync("test=test2"));

            Assert.NotNull(requestInfo.RawQueryParameters);
            Assert.Single(requestInfo.RawQueryParameters);
            Assert.Equal("test=test2", requestInfo.RawQueryParameters.First().SerializeToString(null));
        }

        [Fact]
        public void AddsTwoRawQueryParam()
        {
            var requestInfo = this.Request<ITwoRawQueryStrings>(x => x.FooAsync("test=test2", "test3=test4"));

            Assert.NotNull(requestInfo.RawQueryParameters);
            var rawQueryParameters = requestInfo.RawQueryParameters.ToList();
            Assert.Equal(2, rawQueryParameters.Count);
            Assert.Equal("test=test2", rawQueryParameters[0].SerializeToString(null));
            Assert.Equal("test3=test4", rawQueryParameters[1].SerializeToString(null));
        }

        [Fact]
        public void CallsToStringOnParam()
        {
            var requestInfo = this.Request<ICustomRawQueryString>(x => x.FooAsync(new HasToString()));

            Assert.NotNull(requestInfo.RawQueryParameters);
            var rawQueryParameters = requestInfo.RawQueryParameters.ToList();
            Assert.Single(rawQueryParameters);
            Assert.Equal("HasToString", rawQueryParameters[0].SerializeToString(null));
        }

        [Fact]
        public void SerializeToStringUsesGivenFormatProvider()
        {
            var hasToString = new HasToString();
            var requestInfo = this.Request<ICustomRawQueryString>(x => x.FooAsync(hasToString));

            var formatProvider = new Mock<IFormatProvider>();

            Assert.Single(requestInfo.RawQueryParameters);
            requestInfo.RawQueryParameters.First().SerializeToString(formatProvider.Object);

            Assert.Equal(formatProvider.Object, hasToString.LastFormatProvider);
            //formatProvider.Verify(x => x.GetFormat(typeof(NumberFormatInfo)));
        }
    }
}
