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

namespace RestEaseUnitTests.RequesterTests
{
    public class QueryParameterTests
    {
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
