using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.RequesterTests
{
    public class PathParameterTests
    {
        public class HasToStringToo
        {
            public override string ToString()
            {
                return "HasToStringToo";
            }
        }

        private readonly PublicRequester requester = new PublicRequester(null);

        [Fact]
        public void SubstitutesPathParameters()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{baz}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "yay");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "baz", "woo");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/woo", uri);
        }

        [Fact]
        public void SubstitutesMultiplePathParametersOfTheSameType()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/{bar}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/yay", uri);
        }

        [Fact]
        public void TreatsNullPathParamsAsEmpty()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<int?>(PathSerializationMethod.ToString, "bar", null);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo//baz", uri);
        }

        [Fact]
        public void DoesNotTreatNullPathParamsAsEmptyWhenUsingSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<string>(PathSerializationMethod.Serialized, "bar", null);

            var pathParameterSerializer = new Mock<RequestPathParamSerializer>();
            pathParameterSerializer
                .Setup(x => x.SerializePathParam<string>(null, It.IsAny<RequestPathParamSerializerInfo>()))
                .Returns("foo")
                .Verifiable();
            this.requester.RequestPathParamSerializer = pathParameterSerializer.Object;

            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/foo/baz", uri);

            pathParameterSerializer.VerifyAll();
        }

        [Fact]
        public void EncodesUsingPathEncoding()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter<string>(PathSerializationMethod.ToString, "bar", "a ?b/cé");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a%20%3fb%2fc%c3%a9/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/yay/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void UsesPathParamInPreferenceToPathProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{bar}/baz");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "bar", "woo");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "bar", "yay");
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/woo/baz", uri, ignoreCase: true);
        }

        [Fact]
        public void DisablesUrlEncodingForPathPropertiesIfRequested()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{path}");
            requestInfo.AddPathProperty(PathSerializationMethod.ToString, "path", "a/b+c", urlEncode: false);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a/b+c", uri, ignoreCase: true);
        }

        [Fact]
        public void DisablesUrlEncodingForPathParamsIfRequested()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "/foo/{path}");
            requestInfo.AddPathParameter(PathSerializationMethod.ToString, "path", "a/b+c", urlEncode: false);
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal("/foo/a/b+c", uri, ignoreCase: true);
        }

        [Fact]
        public void ThrowsIfSerializedSerializationMethodUsedButNoRequestPathParamSerializerSet()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "{foo}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "foo", "bar");
            this.requester.RequestPathParamSerializer = null;
            Assert.Throws<InvalidOperationException>(() => this.requester.SubstitutePathParameters(requestInfo));
        }

        [Fact]
        public void SerializesUsingSerializerForSerializedSerializationMethod()
        {
            var obj = new HasToStringToo();
            var requestInfo = new RequestInfo(HttpMethod.Get, "{foo}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "foo", obj);

            var serializer = new Mock<RequestPathParamSerializer>();
            serializer.Setup(x => x.SerializePathParam(obj, It.IsAny<RequestPathParamSerializerInfo>()))
                .Returns("SomethingElse")
                .Verifiable();
            this.requester.RequestPathParamSerializer = serializer.Object;

            var uri = this.requester.SubstitutePathParameters(requestInfo);

            serializer.VerifyAll();
            Assert.Equal("SomethingElse", uri);
        }

        [Fact]
        public void PassesFormatWhenSerializing()
        {
            var obj = new HasToStringToo();
            var requestInfo = new RequestInfo(HttpMethod.Get, "{foo}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "foo", obj, "D5");

            var serializer = new Mock<RequestPathParamSerializer>();
            serializer.Setup(x =>
                    x.SerializePathParam(obj, new RequestPathParamSerializerInfo(requestInfo, "D5", null)))
                .Returns("yep")
                .Verifiable();
            this.requester.RequestPathParamSerializer = serializer.Object;

            var uri = this.requester.SubstitutePathParameters(requestInfo);

            serializer.VerifyAll();
        }

        [Fact]
        public void PassesFormatProviderWhenSerializing()
        {
            var obj = new HasToStringToo();

#if NETCOREAPP1_0
            var provider = new CultureInfo("sv-SE");
#else
            var provider = CultureInfo.GetCultureInfo("sv-SE");
#endif

            this.requester.FormatProvider = provider;
            var requestInfo = new RequestInfo(HttpMethod.Get, "{foo}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "foo", obj);

            var serializer = new Mock<RequestPathParamSerializer>();
            serializer.Setup(x =>
                    x.SerializePathParam(obj, new RequestPathParamSerializerInfo(requestInfo, null, provider)))
                .Returns("yep")
                .Verifiable();
            this.requester.RequestPathParamSerializer = serializer.Object;

            var uri = this.requester.SubstitutePathParameters(requestInfo);

            serializer.VerifyAll();
        }

        [Fact]
        public void DoesNotThrowIfRequestPathParamSerializerReturnsNull()
        {
            var serializer = new Mock<RequestPathParamSerializer>();
            serializer.Setup(x => x.SerializePathParam(It.IsAny<object>(), new RequestPathParamSerializerInfo()))
                .Returns((string)null)
                .Verifiable();

            var requestInfo = new RequestInfo(HttpMethod.Get, "{name}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "name", "value");
            this.requester.RequestPathParamSerializer = serializer.Object;
            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal(string.Empty, uri);
        }

        [Fact]
        public void ShouldSubstituteEmptyStringOnNullValueFromSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "{foo}");
            requestInfo.AddPathParameter(PathSerializationMethod.Serialized, "foo", "fancy");

            var serializer = new Mock<RequestPathParamSerializer>();
            serializer.Setup(x => x.SerializePathParam("fancy", It.IsAny<RequestPathParamSerializerInfo>()))
                .Returns((string)null)
                .Verifiable();
            this.requester.RequestPathParamSerializer = serializer.Object;

            var uri = this.requester.SubstitutePathParameters(requestInfo);
            Assert.Equal(string.Empty, uri);

            serializer.VerifyAll();
        }
    }
}
