using Newtonsoft.Json;
using RestEase.Implementation;
using System;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Creates REST API clients from a suitable interface. Your single point of interaction with RestEase
    /// </summary>
    public static class RestClient
    {
        /// <summary>
        /// Name of the assembly in which interface implementations are built. Use in [assembly: InternalsVisibleTo(RestEase.FactoryAssemblyName)] to allow clients to be generated for internal interface types
        /// </summary>
        public const string FactoryAssemblyName = "RestEaseFactory";

        private static readonly ImplementationBuilder implementationBuilder = new ImplementationBuilder();

        private static HttpClient CreateClient(string baseUrl, RequestModifier requestModifier = null)
        {
            var httpClient = requestModifier == null ?
                new HttpClient() :
                new HttpClient(new ModifyingClientHttpHandler(requestModifier));

            httpClient.BaseAddress = new Uri(baseUrl);

            return httpClient;
        }

        /// <summary>
        /// Create a client using the given url
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl)
        {
            return For<T>(CreateClient(baseUrl));
        }

        /// <summary>
        /// Create a client using the given URL and request interceptor
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestInterceptor">Delegate called on every request</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, RequestModifier requestInterceptor)
        {
            return For<T>(CreateClient(baseUrl, requestInterceptor));
        }

        /// <summary>
        /// Create a client using the given base URL and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, JsonSerializerSettings jsonSerializerSettings)
        {
            return For<T>(CreateClient(baseUrl), jsonSerializerSettings);
        }

        /// <summary>
        /// Create a client using the given base URL and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestInterceptor">Delegate called on every request</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, RequestModifier requestInterceptor, JsonSerializerSettings jsonSerializerSettings)
        {
            return For<T>(CreateClient(baseUrl, requestInterceptor), jsonSerializerSettings);
        }

        /// <summary>
        /// Create a client using the given base URL, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            return For<T>(CreateClient(baseUrl), responseDeserializer, requestSerializer);
        }

        /// <summary>
        /// Create a client using the given base URL, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestInterceptor">Delegate called on every request</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, RequestModifier requestInterceptor, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            return For<T>(CreateClient(baseUrl, requestInterceptor), responseDeserializer, requestSerializer);
        }

        /// <summary>
        /// Create a client using the given HttpClient
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(HttpClient httpClient)
        {
            return For<T>(httpClient, jsonSerializerSettings: null);
        }

        /// <summary>
        /// Create a client using the given HttpClient and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(HttpClient httpClient, JsonSerializerSettings jsonSerializerSettings)
        {
            var responseDeserializer = new JsonResponseDeserializer()
            {
                JsonSerializerSettings = jsonSerializerSettings,
            };
            var requestSerializer = new JsonRequestSerializer()
            {
                JsonSerializerSettings = jsonSerializerSettings,
            };

            return For<T>(httpClient, responseDeserializer, requestSerializer);
        }

        /// <summary>
        /// Create a client using the given HttpClient, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(HttpClient httpClient, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            var requester = new Requester(httpClient);

            if (responseDeserializer != null)
                requester.ResponseDeserializer = responseDeserializer;
            if (requestSerializer != null)
                requester.RequestSerializer = requestSerializer;

            return For<T>(requester);
        }

        /// <summary>
        /// Create a client using the given IRequester. This gives you the greatest ability to customise functionality
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="requester">IRequester to use</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(IRequester requester)
        {
            return implementationBuilder.CreateImplementation<T>(requester);
        }
    }
}
