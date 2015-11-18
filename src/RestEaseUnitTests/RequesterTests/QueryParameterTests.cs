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
        public void IgnoresNullQueryParamsWhenUsingToString()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter<object>(QuerySerializationMethod.ToString, "bar", null);
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo"), uri);
        }

        [Fact]
        public void DoesNotIgnoreNullQueryParamsWhenUsingSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter<object>(QuerySerializationMethod.Serialized, "bar", null);

            var queryParameterSerializer = new Mock<IRequestQueryParamSerializer>();
            queryParameterSerializer.Setup(x => x.SerializeQueryParam<object>("bar", null))
                .Returns(new[] { new KeyValuePair<string, string>("bar", "foo") })
                .Verifiable();
            this.requester.RequestQueryParamSerializer = queryParameterSerializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=foo"), uri);

            queryParameterSerializer.VerifyAll();
        }

        [Fact]
        public void IgnoresNullQueryParamArraysWhenUsingToString()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryCollectionParameter<object>(QuerySerializationMethod.ToString, "foo", null);
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo"), uri);
        }

        [Fact]
        public void IgnoresNullQueryParamArrayValuesWhenUsingToString()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryCollectionParameter<object>(QuerySerializationMethod.ToString, "foo", new[] { "bar", null, "baz" });
            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?foo=bar&foo=baz"), uri);
        }

        [Fact]
        public void DoesNotIgnoreNullQueryParamArraysWhenUsingSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryCollectionParameter<object>(QuerySerializationMethod.Serialized, "bar", null);

            var queryParameterSerializer = new Mock<IRequestQueryParamSerializer>();
            queryParameterSerializer.Setup(x => x.SerializeQueryCollectionParam<object>("bar", null))
                .Returns(new[] { new KeyValuePair<string, string>("bar", "foo") })
                .Verifiable();
            this.requester.RequestQueryParamSerializer = queryParameterSerializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=foo"), uri);

            queryParameterSerializer.VerifyAll();
        }

        [Fact]
        public void DoesNotIgoreNullQueryParamArrayValuesWhenUsingSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryCollectionParameter<object>(QuerySerializationMethod.Serialized, "bar", new[] { "baz", null, "yay" });

            var queryParameterSerializer = new Mock<IRequestQueryParamSerializer>();
            queryParameterSerializer.Setup(x => x.SerializeQueryCollectionParam<object>("bar", new[] { "baz", null, "yay" }))
                .Returns(new[] { new KeyValuePair<string, string>("bar", "foo"), new KeyValuePair<string, string>("bar", "baz") })
                .Verifiable();
            this.requester.RequestQueryParamSerializer = queryParameterSerializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=foo&bar=baz"), uri);

            queryParameterSerializer.VerifyAll();
        }

        [Fact]
        public void CallsToStringForToStringSerializationMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var objectMock = new Mock<HasToString>();
            objectMock.Setup(x => x.ToString()).Returns("BOOM").Verifiable();
            requestInfo.AddQueryParameter(QuerySerializationMethod.ToString, "bar", objectMock.Object);
            var uri = this.requester.ConstructUri("/foo", requestInfo);

            objectMock.VerifyAll();
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOM"), uri);
        }

        [Fact]
        public void CallsToStringOnEachNonNullElementForToStringSerializationMethodOnCollections()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            var objectMock1 = new Mock<HasToString>();
            var objectMock2 = new Mock<HasToString>();
            objectMock1.Setup(x => x.ToString()).Returns("BOOM1").Verifiable();
            objectMock2.Setup(x => x.ToString()).Returns("BOOM2").Verifiable();
            requestInfo.AddQueryCollectionParameter(QuerySerializationMethod.ToString, "bar", new[] { objectMock1.Object, null, objectMock2.Object });
            var uri = this.requester.ConstructUri("/foo", requestInfo);

            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOM1&bar=BOOM2"), uri);

            objectMock1.VerifyAll();
            objectMock2.VerifyAll();
        }

        [Fact]
        public void ThrowsIfSerializedSerializationMethodUsedButNoRequestQueryParamSerializerSet()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.Serialized, "bar", "boom");
            this.requester.RequestQueryParamSerializer = null;
            Assert.Throws<InvalidOperationException>(() => this.requester.ConstructUri("/foo", requestInfo));
        }

        [Fact]
        public void SerializesUsingSerializerForSerializedSerializationMethod()
        {
            var obj = new HasToString();
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryParameter(QuerySerializationMethod.Serialized, "bar", obj);

            var serializer = new Mock<IRequestQueryParamSerializer>();
            serializer.Setup(x => x.SerializeQueryParam<HasToString>("bar", obj)).Returns(new[] { new KeyValuePair<string, string>("bar", "BOOMYAY") }).Verifiable();
            this.requester.RequestQueryParamSerializer = serializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);

            serializer.VerifyAll();
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOMYAY"), uri);
        }

        [Fact]
        public void SerializesUsingSerializerForSerializedSerializationMethodOnCollections()
        {
            var obj = new HasToString();
            var requestInfo = new RequestInfo(HttpMethod.Get, null);
            requestInfo.AddQueryCollectionParameter(QuerySerializationMethod.Serialized, "bar", new[] { obj });

            var serializer = new Mock<IRequestQueryParamSerializer>();
            serializer.Setup(x => x.SerializeQueryCollectionParam<HasToString>("bar", new[] { obj }))
                .Returns(new[] { new KeyValuePair<string, string>("bar", "BOOMYAY"), new KeyValuePair<string, string>("bar", "BOOMWOO") })
                .Verifiable();
            this.requester.RequestQueryParamSerializer = serializer.Object;

            var uri = this.requester.ConstructUri("/foo", requestInfo);

            serializer.VerifyAll();
            Assert.Equal(new Uri("http://api.example.com/base/foo?bar=BOOMYAY&bar=BOOMWOO"), uri);
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
            requestInfo.QueryMap = new Dictionary<string, object>()
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
