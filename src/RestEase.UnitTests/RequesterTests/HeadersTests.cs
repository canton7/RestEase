using System;
using System.Collections.Generic;
using System.Net.Http;
using RestEase;
using RestEase.Implementation;
using Xunit;

namespace RestEase.UnitTests.RequesterTests
{
    public class HeadersTests
    {
        private readonly PublicRequester requester = new(null);

        [Fact]
        public void AppliesHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                ClassHeaders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("User-Agent", "RestEase"),
                    new KeyValuePair<string, string>("X-API-Key", "Foo"),
                }
            };

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddPropertyHeader("User-Agent", "RestEase", null);
            requestInfo.AddPropertyHeader("X-API-Key", "Foo", null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("User-Agent", "RestEase");
            requestInfo.AddMethodHeader("X-API-Key", "Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesHeadersFromParams()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddHeaderParameter("User-Agent", "RestEase");
            requestInfo.AddHeaderParameter("X-API-Key", "Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void HeadersFromPropertiesCombineWithHeadersFromClass()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                ClassHeaders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("This-Will-Stay", "YesIWill"),
                    new KeyValuePair<string, string>("Something", "SomethingElse"),
                    new KeyValuePair<string, string>("User-Agent", "RestEase"),
                    new KeyValuePair<string, string>("X-API-Key", "Foo"),
                }
            };

            requestInfo.AddPropertyHeader<string>("Something", null, null);
            requestInfo.AddPropertyHeader("User-Agent", string.Empty, null);
            requestInfo.AddPropertyHeader("X-API-Key", "Bar", null);
            requestInfo.AddPropertyHeader("This-Is-New", "YesIAm", null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal(new[] { "YesIWill" }, message.Headers.GetValues("This-Will-Stay"));
            Assert.Equal(new[] { "SomethingElse" }, message.Headers.GetValues("Something"));
            Assert.Equal(new[] { "RestEase", "" }, message.Headers.GetValues("User-Agent"));
            Assert.Equal(new[] { "Foo", "Bar" }, message.Headers.GetValues("X-API-Key"));
            Assert.Equal(new[] { "YesIAm" }, message.Headers.GetValues("This-Is-New"));
        }

        [Fact]
        public void HeadersFromMethodOverrideHeadersFromProperties()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddPropertyHeader("This-Will-Stay", "YesIWill", null);
            requestInfo.AddPropertyHeader("Something", "SomethingElse", null);
            requestInfo.AddPropertyHeader("User-Agent", "RestEase", null);
            requestInfo.AddPropertyHeader("X-API-Key", "Foo", null);

            requestInfo.AddMethodHeader("Something", null); // Remove
            requestInfo.AddMethodHeader("User-Agent", ""); // Replace with null
            requestInfo.AddMethodHeader("X-API-Key", "Bar"); // Change value
            requestInfo.AddMethodHeader("This-Is-New", "YesIAM"); // New value

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("This-Will-Stay: YesIWill\r\nThis-Is-New: YesIAM\r\nUser-Agent: \r\nX-API-Key: Bar\r\n", message.Headers.ToString());
        }

        [Fact]
        public void HeadersFromParamsCombineWithHeadersFromMethod()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("This-Will-Stay", "YesIWill");
            requestInfo.AddMethodHeader("Something", "SomethingElse");
            requestInfo.AddMethodHeader("User-Agent", "RestEase");
            requestInfo.AddMethodHeader("X-API-Key", "Foo");

            requestInfo.AddHeaderParameter<string>("Something", null);
            requestInfo.AddHeaderParameter("User-Agent", "");
            requestInfo.AddHeaderParameter("X-API-Key", "Bar");
            requestInfo.AddHeaderParameter("This-Is-New", "YesIAm");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal(new[] { "YesIWill" }, message.Headers.GetValues("This-Will-Stay"));
            Assert.Equal(new[] { "SomethingElse" }, message.Headers.GetValues("Something"));
            Assert.Equal(new[] { "RestEase", "" }, message.Headers.GetValues("User-Agent"));
            Assert.Equal(new[] { "Foo", "Bar" }, message.Headers.GetValues("X-API-Key"));
            Assert.Equal(new[] { "YesIAm" }, message.Headers.GetValues("This-Is-New"));
        }

        [Fact]
        public void AppliesHeadersFromSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.SetBodyParameterInfo<object>(BodySerializationMethod.Serialized, new object());

            var message = new HttpRequestMessage
            {
                Content = this.requester.ConstructContent(requestInfo)
            };
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("Content-Type: application/json; charset=utf-8\r\n", message.Content.Headers.ToString());
        }

        [Fact]
        public void HeadersFromClassOverrideHeadersFromSerializer()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                ClassHeaders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Content-Type", "foo/bar"),
                }
            };
            requestInfo.SetBodyParameterInfo<object>(BodySerializationMethod.Serialized, new object());

            var message = new HttpRequestMessage
            {
                Content = this.requester.ConstructContent(requestInfo)
            };
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("Content-Type: foo/bar\r\n", message.Content.Headers.ToString());
        }

        [Fact]
        public void MultipleHeadersAreAllowed()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("User-Agent", "SomethingElse");
            requestInfo.AddMethodHeader("User-Agent", "RestEase");
            requestInfo.AddMethodHeader("X-API-Key", "Foo");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("User-Agent: SomethingElse RestEase\r\nX-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void SingleOverrideReplacesMultipleHeaders()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo")
            {
                ClassHeaders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("User-Agent", "SomethingElse"),
                    new KeyValuePair<string, string>("User-Agent", "RestEase"),
                    new KeyValuePair<string, string>("X-API-Key", "Foo"),
                }
            };

            requestInfo.AddHeaderParameter<object>("User-Agent", null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("X-API-Key: Foo\r\n", message.Headers.ToString());
        }

        [Fact]
        public void AppliesContentHeadersToContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("Content-Type", "text/html");

            var message = new HttpRequestMessage
            {
                Content = new StringContent("hello")
            };
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Equal("", message.Headers.ToString());
            Assert.Equal("Content-Type: text/html\r\n", message.Content.Headers.ToString());
        }

        [Fact]
        public void IgnoresContentHeaderAppliedToClassButThereIsNoContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddPropertyHeader("Content-Type", "text/html", string.Empty);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.Null(message.Content);
        }

        [Fact]
        public void AddsContentIfContentHeaderAppliedToMethodButThereIsNoContent()
        {
            var requestInfo = new RequestInfo(HttpMethod.Get, "foo");
            requestInfo.AddMethodHeader("Content-Type", "text/html");

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.NotNull(message.Content);
            Assert.Equal(0, message.Content.Headers.ContentLength);
            Assert.Equal("text/html", message.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void AddsContentHeadersOnClassIfBodyIsNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Post, "foo")
            {
                ClassHeaders = new[]
                {
                    new KeyValuePair<string, string>("Content-Type", "text/plain"),
                }
            };
            requestInfo.SetBodyParameterInfo<object>(BodySerializationMethod.Default, null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.NotNull(message.Content);
            Assert.Equal("text/plain", message.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void AddsContentHeadersOnMethodIfBodyIsNull()
        {
            var requestInfo = new RequestInfo(HttpMethod.Post, "foo");
            requestInfo.AddMethodHeader("Content-Type", "text/plain");
            requestInfo.SetBodyParameterInfo<object>(BodySerializationMethod.Default, null);

            var message = new HttpRequestMessage();
            this.requester.ApplyHeaders(requestInfo, message);

            Assert.NotNull(message.Content);
            Assert.Equal("text/plain", message.Content.Headers.ContentType.MediaType);
        }
    }
}
