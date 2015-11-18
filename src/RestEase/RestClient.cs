using Newtonsoft.Json;
using RestEase.Implementation;
using System;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Creates REST API clients from a suitable interface. Your single point of interaction with RestEase
    /// </summary>
    public class RestClient
    {
        /// <summary>
        /// Name of the assembly in which interface implementations are built. Use in [assembly: InternalsVisibleTo(RestEase.FactoryAssemblyName)] to allow clients to be generated for internal interface types
        /// </summary>
        public const string FactoryAssemblyName = "RestEaseFactory";

        private static readonly ImplementationBuilder implementationBuilder = new ImplementationBuilder();

        private readonly HttpClient httpClient;

        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        public IRequestSerializer RequestSerializer { get; set; }
        public IResponseDeserializer ResponseDeserializer { get; set; }

        public RestClient(string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException("baseUrl");

            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public RestClient(string baseUrl, RequestModifier requestModifier)
        {
            if (baseUrl == null)
                throw new ArgumentNullException("baseUrl");
            if (requestModifier == null)
                throw new ArgumentNullException("requestModifier");

            this.httpClient = new HttpClient(new ModifyingClientHttpHandler(requestModifier))
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public RestClient(HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException("httpClient");

            this.httpClient = httpClient;
        }

        public T For<T>()
        {
            var requestSerializer = this.RequestSerializer ?? new JsonRequestSerializer() { JsonSerializerSettings = this.JsonSerializerSettings };
            var responseDeserializer = this.ResponseDeserializer ?? new JsonResponseDeserializer() { JsonSerializerSettings = this.JsonSerializerSettings };

            var requester = new Requester(this.httpClient)
            {
                RequestSerializer = requestSerializer,
                ResponseDeserializer = responseDeserializer,
            };

            return implementationBuilder.CreateImplementation<T>(requester);
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

        /// <summary>
        /// Shortcut to create a client using the given url
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl)
        {
            return new RestClient(baseUrl).For<T>();
        }

        /// <summary>
        /// Shortcut to create a client using the given HttpClient
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(HttpClient httpClient)
        {
            return new RestClient(httpClient).For<T>();
        }

        /// <summary>
        /// Shortcut to create a client using the given URL and request interceptor
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(string baseUrl, RequestModifier requestModifier)
        {
            return new RestClient(baseUrl, requestModifier).For<T>();
        }

        /// <summary>
        /// Create a client using the given base URL and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl) { JsonSerializerSettings = jsonSerializerSettings }.For<T>()' instead")]
        public static T For<T>(string baseUrl, JsonSerializerSettings jsonSerializerSettings)
        {
            return new RestClient(baseUrl) { JsonSerializerSettings = jsonSerializerSettings }.For<T>();
        }

        /// <summary>
        /// Create a client using the given base URL and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl, requestModifier) { JsonSerializerSettings = jsonSerializerSettings }.For<T>()' instead")]
        public static T For<T>(string baseUrl, RequestModifier requestModifier, JsonSerializerSettings jsonSerializerSettings)
        {
            return new RestClient(baseUrl, requestModifier) { JsonSerializerSettings = jsonSerializerSettings }.For<T>();
        }

        /// <summary>
        /// Create a client using the given base URL, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>()' instead")]
        public static T For<T>(string baseUrl, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            return new RestClient(baseUrl) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>();
        }

        /// <summary>
        /// Create a client using the given base URL, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl, requestModifier) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>()' instead")]
        public static T For<T>(string baseUrl, RequestModifier requestModifier, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            return new RestClient(baseUrl, requestModifier) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>();
        }

        /// <summary>
        /// Create a client using the given HttpClient and Json.NET serializer settings
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <param name="jsonSerializerSettings">Serializer settings to pass to Json.NET</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(httpClient) { JsonSerializerSettings = jsonSerializerSettings }.For<T>()' instead")] 
        public static T For<T>(HttpClient httpClient, JsonSerializerSettings jsonSerializerSettings)
        {
            return new RestClient(httpClient) { JsonSerializerSettings = jsonSerializerSettings }.For<T>();
        }

        /// <summary>
        /// Create a client using the given HttpClient, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestSerializer">Serializer to use when serializing request bodies or query parameters, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(httpClient) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>()' instead")]
        public static T For<T>(HttpClient httpClient, IResponseDeserializer responseDeserializer = null, IRequestSerializer requestSerializer = null)
        {
            return new RestClient(httpClient) { ResponseDeserializer = responseDeserializer, RequestSerializer = requestSerializer }.For<T>();
        }
    }
}
