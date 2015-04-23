using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase
{
    public static class RestService
    {
        private static readonly ImplementationBuilder implementationBuilder = new ImplementationBuilder();

        private static HttpClient CreateClient(string baseUrl)
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
            };
        }

        public static T For<T>(string baseUrl)
        {
            return For<T>(CreateClient(baseUrl));
        }

        public static T For<T>(string baseUrl, RequestInterceptor requestInterceptor)
        {
            var httpClient = new HttpClient(new InterceptingHttpClientHandler(requestInterceptor))
            {
                BaseAddress = new Uri(baseUrl),
            };

            return For<T>(httpClient);
        }

        public static T For<T>(HttpClient httpClient)
        {
            return For<T>(httpClient, jsonSerializerSettings: null);
        }

        public static T For<T>(string baseUrl, JsonSerializerSettings jsonSerializerSettings)
        {
            return For<T>(CreateClient(baseUrl), jsonSerializerSettings);
        }

        public static T For<T>(HttpClient httpClient, JsonSerializerSettings jsonSerializerSettings)
        {
            var responseDeserializer = new JsonResponseDeserializer()
            {
                JsonSerializerSettings = jsonSerializerSettings,
            };
            var requestBodySerializer = new JsonRequestBodySerializer()
            {
                JsonSerializerSettings = jsonSerializerSettings,
            };

            return For<T>(httpClient, responseDeserializer, requestBodySerializer);
        }

        public static T For<T>(string baseUrl, IResponseDeserializer responseDeserializer = null, IRequestBodySerializer requestBodySerializer = null)
        {
            return For<T>(CreateClient(baseUrl), responseDeserializer, requestBodySerializer);
        }

        public static T For<T>(HttpClient httpClient, IResponseDeserializer responseDeserializer = null, IRequestBodySerializer requestBodySerializer = null)
        {
            var requester = new Requester(httpClient);

            if (responseDeserializer != null)
                requester.ResponseDeserializer = responseDeserializer;
            if (requestBodySerializer != null)
                requester.RequestBodySerializer = requestBodySerializer;

            return For<T>(requester);
        }

        public static T For<T>(IRequester requester)
        {
            return implementationBuilder.CreateImplementation<T>(requester);
        }
    }
}
