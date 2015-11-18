using RestEase.Implementation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RestEase;
using Moq;

namespace RestEaseUnitTests.RequesterTests
{
    public class QueryParameterTests
    {
        // See http://stackoverflow.com/q/16979196/1086121
        public class HasToString
        {
            public override string ToString()
            {
                return "HasToString";
            }
        }

        private readonly PublicRequester requester = new PublicRequester(new HttpClient() { BaseAddress = new Uri("http://api.example.com/base") });

        [Fact]
        public void IgnoresNullQueryParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter<object>(QuerySerialializationMethod.ToString, "bar", null);
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo"), uri);
        }

        [Fact]
        public void CallsToStringForToStringSerializationMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var objectMock = new Mock<HasToString>();
            objectMock.Setup(x => x.ToString()).Returns("BOOM").Verifiable();
            requestInfo.AddQueryParameter(QuerySerialializationMethod.ToString, "bar", objectMock.Object);
            var uri = this.requester.ConstructUri("/foo", requestInfo);

            objectMock.VerifyAll();
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOM"), uri);
        }

        [Fact]
        public void ThrowsIfSerializedSerializationMethodUsedButNoRequestQueryParamSerializerSet()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerialializationMethod.Serialized, "bar", "boom");
            this.requester.RequestQueryParamSerializer = null;
            Assert.Throws<InvalidOperationException>(() => this.requester.ConstructUri("/foo", requestInfo));
        }

        [Fact]
        public void SerializesUsingSerializerForSerializedSerializationMethod()
        {
            var obj = new HasToString();
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerialializationMethod.Serialized, "bar", obj);

            var serializer = new Mock<IRequestQueryParamSerializer>();
            serializer.Setup(x => x.SerializeQueryParam<HasToString>("bar", obj)).Returns(new[] { new KeyValuePair<string, string>("bar", "BOOMYAY") }).Verifiable();
            this.requester.RequestQueryParamSerializer = serializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);

            serializer.VerifyAll();
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOMYAY"), uri);
        }

        [Fact]
        public void AddsParamsFromGenericQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            // Use an ExpandoObject, as it implements IDictionary<string, object> but *not* IDictionary
            dynamic queryMap = new ExpandoObject();
            queryMap.foo = "bar";
            queryMap.baz = "yay";
            requestInfo.QueryMap = queryMap;
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?foo=bar&baz=yay"), uri);
        }

        [Fact]
        public void AddsParamsFromNonGenericQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = (IDictionary)new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "baz", "yay" },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?foo=bar&baz=yay"), uri);
        }

        [Fact]
        public void IgnoresNullItemsFromQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "baz", null },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?foo=bar"), uri);
        }

        [Fact]
        public void HandlesArraysInQueryMap()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.QueryMap = new Dictionary<string, object>()
            {
                { "foo", new[] { "bar", "baz" } },
            };
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?foo=bar&foo=baz"), uri);
        }
    }
}
