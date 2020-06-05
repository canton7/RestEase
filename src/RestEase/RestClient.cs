using Newtonsoft.Json;
using RestEase.Implementation;
using RestEase.Platform;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Collections.Generic;

namespace RestEase
{
    /// <summary>
    /// Creates REST API clients from a suitable interface. Your single point of interaction with RestEase
    /// </summary>
    public class RestClient
    {
        private static readonly MethodInfo forInstanceGenericMethodInfo = typeof(RestClient).GetTypeInfo().GetMethods().First(x => x.Name == "For" && !x.IsStatic && x.GetParameters().Length == 0 && x.IsGenericMethod);
        private static readonly MethodInfo forStaticGenericMethodInfo = typeof(RestClient).GetTypeInfo().GetMethods().First(x => x.Name == "For" && x.IsStatic && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IRequester) && x.IsGenericMethod);

        private static readonly ImplementationFactory factory = new ImplementationFactory();

        /// <summary>
        /// Name of the assembly in which interface implementations are built. Use in [assembly: InternalsVisibleTo(RestEase.FactoryAssemblyName)] to allow clients to be generated for internal interface types
        /// </summary>
        public const string FactoryAssemblyName = "RestEaseFactory";

        /// <summary>
        /// Name of the key in <see cref="HttpRequestMessage.Properties"/> that a request's <see cref="IRequestInfo"/> is stored
        /// </summary>
        public const string HttpRequestMessageRequestInfoPropertyKey = "RestEaseRequestInfo";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Gets or sets the JsonSerializerSettings to use with all non-overridden serializers / deserializers
        /// </summary>
        public JsonSerializerSettings? JsonSerializerSettings { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets or sets the deserializer used to deserialize responses
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="JsonResponseDeserializer"/>.
        ///
        /// This has the type <see cref="IResponseDeserializer"/> for backwards-compatibility reasons. You should assign
        /// an instance of <see cref="ResponseDeserializer"/>
        /// </remarks>
        public IResponseDeserializer? ResponseDeserializer { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize request bodies (when [Body(BodySerializationMethod.Serialized)] is used)
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="JsonRequestBodySerializer"/>.
        ///
        /// This has the type <see cref="IRequestBodySerializer"/> for backwards-compatibility reasons. You should assign
        /// an instance of <see cref="RequestBodySerializer"/>.
        /// </remarks>
        public IRequestBodySerializer? RequestBodySerializer { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize path parameters (when [Path(PathSerializationMethod.Serialized)] is used)
        /// </summary>
        /// <remarks>
        /// Has no default value, explicit serializer implementation must be provided.
        /// </remarks>
        public RequestPathParamSerializer? RequestPathParamSerializer { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize query parameters (when [Query(QuerySerializationMethod.Serialized)] is used)
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="JsonRequestQueryParamSerializer"/>.
        ///
        /// This has the type <see cref="IRequestQueryParamSerializer"/> for backwards-compatibility reasons. You should assign
        /// an instance of <see cref="RequestQueryParamSerializer"/>.
        /// </remarks>
        public IRequestQueryParamSerializer? RequestQueryParamSerializer { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets or sets the builder to use to construct encoded query strings from various parmaeters
        /// </summary>
        /// <remarks>
        /// Defaults to null, in which case the default building logic is used
        /// </remarks>
        public QueryStringBuilder? QueryStringBuilder { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFormatProvider"/> used to format items using <see cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// </summary>
        /// <remarks>
        /// Defaults to null, in which case the current culture is used.
        /// </remarks>
        public IFormatProvider? FormatProvider { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClient"/> class, with the given Base URL
        /// </summary>
        /// <param name="baseUrl">Base URL of the API</param>
        public RestClient(string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            this.httpClient = this.Initialize(new HttpClientHandler(), new Uri(baseUrl));
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClient"/> class, with the given Base URL
        /// </summary>
        /// <param name="baseUrl">Base URL of the API</param>
        public RestClient(Uri baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            this.httpClient = this.Initialize(new HttpClientHandler(), baseUrl);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClient"/> class, with the given Base URL and request modifier
        /// </summary>
        /// <param name="baseUrl">Base URL of the API</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        public RestClient(string baseUrl, RequestModifier requestModifier)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));
            if (requestModifier == null)
                throw new ArgumentNullException(nameof(requestModifier));

            this.httpClient = this.Initialize(new ModifyingClientHttpHandler(requestModifier), new Uri(baseUrl));
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClient"/> class, with the given Base URL and request modifier
        /// </summary>
        /// <param name="baseUrl">Base URL of the API</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        public RestClient(Uri baseUrl, RequestModifier requestModifier)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));
            if (requestModifier == null)
                throw new ArgumentNullException(nameof(requestModifier));

            this.httpClient = this.Initialize(new ModifyingClientHttpHandler(requestModifier), baseUrl);
        }

        private HttpClient Initialize(HttpMessageHandler messageHandler, Uri baseUrl)
        {
            return new HttpClient(messageHandler)
            {
                BaseAddress = baseUrl,
            };
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClient"/> class, using the given HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient to use</param>
        public RestClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Create an implementation for the given API interface
        /// </summary>
        /// <param name="type">Type of interface to implement</param>
        /// <returns>An implementation which can be used to make REST requests</returns>
        public object For(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var method = forInstanceGenericMethodInfo.MakeGenericMethod(type);
            return method.Invoke(this, new object[0]);
        }

        /// <summary>
        /// Create an implementation for the given API interface
        /// </summary>
        /// <typeparam name="T">Type of interface to implement</typeparam>
        /// <returns>An implementation which can be used to make REST requests</returns>
        public T For<T>()
        {
            var requester = this.CreateRequester();
            return factory.CreateImplementation<T>(requester);
        }

        private Requester CreateRequester()
        {
            var requester = new Requester(this.httpClient);

            if (this.RequestBodySerializer is RequestBodySerializer requestBodySerializer)
                requester.RequestBodySerializer = requestBodySerializer;
            else if (this.RequestBodySerializer != null)
                requester.RequestBodySerializer = new RequestBodySerializerWrapper(this.RequestBodySerializer);
            else if (this.JsonSerializerSettings != null)
                requester.RequestBodySerializer = new JsonRequestBodySerializer() { JsonSerializerSettings = this.JsonSerializerSettings };

            requester.RequestPathParamSerializer = this.RequestPathParamSerializer;

            if (this.RequestQueryParamSerializer is RequestQueryParamSerializer requestQueryParamSerializer)
                requester.RequestQueryParamSerializer = requestQueryParamSerializer;
            else if (this.RequestQueryParamSerializer != null)
                requester.RequestQueryParamSerializer = new RequestQueryParamSerializerWrapper(this.RequestQueryParamSerializer);
            else if (this.JsonSerializerSettings != null)
                requester.RequestQueryParamSerializer = new JsonRequestQueryParamSerializer() { JsonSerializerSettings = this.JsonSerializerSettings };

            if (this.ResponseDeserializer is ResponseDeserializer responseDeserializer)
                requester.ResponseDeserializer = responseDeserializer;
            else if (this.ResponseDeserializer != null)
                requester.ResponseDeserializer = new ResponseDeserializerWrapper(this.ResponseDeserializer);
            else if (this.JsonSerializerSettings != null)
                requester.ResponseDeserializer = new JsonResponseDeserializer() { JsonSerializerSettings = this.JsonSerializerSettings };

            requester.QueryStringBuilder = this.QueryStringBuilder;

            requester.FormatProvider = this.FormatProvider;

            return requester;
        }

        /// <summary>
        /// Create a client using the given IRequester. This gives you the greatest ability to customise functionality
        /// </summary>
        /// <param name="type">Interface representing the API</param>
        /// <param name="requester">IRequester to use</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static object For(Type type, IRequester requester)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var method = forStaticGenericMethodInfo.MakeGenericMethod(type);
            return method.Invoke(null, new object[] { requester });
        }

        /// <summary>
        /// Create a client using the given IRequester. This gives you the greatest ability to customise functionality
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="requester">IRequester to use</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(IRequester requester)
        {
            return factory.CreateImplementation<T>(requester);
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
        /// Shortcut to create a client using the given url
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(Uri baseUrl)
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
        /// Shortcut to create a client using the given URL and request interceptor
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        public static T For<T>(Uri baseUrl, RequestModifier requestModifier)
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
        /// <param name="requestBodySerializer">Serializer to use when serializing request bodies, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>()' instead")]
        public static T For<T>(string baseUrl, ResponseDeserializer? responseDeserializer = null, RequestBodySerializer? requestBodySerializer = null)
        {
            return new RestClient(baseUrl) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>();
        }

        /// <summary>
        /// Create a client using the given base URL, and custom serializer and/or deserializer
        /// </summary>
        /// <typeparam name="T">Interface representing the API</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="requestModifier">Delegate called on every request</param>
        /// <param name="responseDeserializer">Deserializer to use when deserializing responses</param>
        /// <param name="requestBodySerializer">Serializer to use when serializing request bodiess, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(baseUrl, requestModifier) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>()' instead")]
        public static T For<T>(string baseUrl, RequestModifier requestModifier, ResponseDeserializer? responseDeserializer = null, RequestBodySerializer? requestBodySerializer = null)
        {
            return new RestClient(baseUrl, requestModifier) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>();
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
        /// <param name="requestBodySerializer">Serializer to use when serializing request bodies, when appropriate</param>
        /// <returns>An implementation of that interface which you can use to invoke the API</returns>
        [Obsolete("Use 'new RestClient(httpClient) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>()' instead")]
        public static T For<T>(HttpClient httpClient, ResponseDeserializer? responseDeserializer = null, RequestBodySerializer? requestBodySerializer = null)
        {
            return new RestClient(httpClient) { ResponseDeserializer = responseDeserializer, RequestBodySerializer = requestBodySerializer }.For<T>();
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private class RequestBodySerializerWrapper : RequestBodySerializer
        {
            private readonly IRequestBodySerializer serializer;

            public RequestBodySerializerWrapper(IRequestBodySerializer serializer) =>

                this.serializer = serializer;

            public override HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info) =>
                this.serializer.SerializeBody(body);
        }

        private class RequestQueryParamSerializerWrapper : RequestQueryParamSerializer
        {
            private readonly IRequestQueryParamSerializer serializer;

            public RequestQueryParamSerializerWrapper(IRequestQueryParamSerializer serializer) =>
                this.serializer = serializer;

            public override IEnumerable<KeyValuePair<string, string?>> SerializeQueryCollectionParam<T>(string name, IEnumerable<T> values, RequestQueryParamSerializerInfo info) =>
                this.serializer.SerializeQueryCollectionParam(name, values, info);

            public override IEnumerable<KeyValuePair<string, string?>> SerializeQueryParam<T>(string name, T value, RequestQueryParamSerializerInfo info) =>
                this.serializer.SerializeQueryParam(name, value, info);
        }

        private class ResponseDeserializerWrapper : ResponseDeserializer
        {
            private readonly IResponseDeserializer deserializer;

            public ResponseDeserializerWrapper(IResponseDeserializer deserializer) =>
                this.deserializer = deserializer;

            public override T Deserialize<T>(string? content, HttpResponseMessage response, ResponseDeserializerInfo info) =>
                this.deserializer.Deserialize<T>(content, response);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
