using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information to construct a request from.
    /// An instance of this is created per request by the generated interface implementation
    /// </summary>
    public class RequestInfo : IRequestInfo
    {
        /// <summary>
        /// Gets the HttpMethod which should be used to make the request
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Gets the relative path to the resource to request
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets or sets the CancellationToken used to cancel the request
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Gets or sets a value indicating whether to suppress the exception on invalid status codes
        /// </summary>
        public bool AllowAnyStatusCode { get; set; }

        private List<QueryParameterInfo> _queryParams;

        /// <summary>
        /// Gets the query parameters to append to the request URI
        /// </summary>
        public IEnumerable<QueryParameterInfo> QueryParams => this._queryParams ?? Enumerable.Empty<QueryParameterInfo>();

        /// <summary>
        /// Gets the raw query parameter provider
        /// </summary>
        public RawQueryParameterInfo RawQueryParameter { get; private set; }

        private List<PathParameterInfo> _pathParams;

        /// <summary>
        /// Gets the parameters which should be substituted into placeholders in the Path
        /// </summary>
        public IEnumerable<PathParameterInfo> PathParams => this._pathParams ?? Enumerable.Empty<PathParameterInfo>();

        private List<PathParameterInfo> _pathProperties;

        /// <summary>
        /// Gets the values from properties which should be substituted into placeholders in the Path
        /// </summary>
        public IEnumerable<PathParameterInfo> PathProperties => this._pathProperties ?? Enumerable.Empty<PathParameterInfo>();

        private List<QueryParameterInfo> _queryProperties;

        /// <summary>
        /// Gets the values from properties which should be added to all query strings
        /// </summary>
        public IEnumerable<QueryParameterInfo> QueryProperties => this._queryProperties ?? Enumerable.Empty<QueryParameterInfo>();

        /// <summary>
        /// Gets or sets the headers which were applied to the interface
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ClassHeaders { get; set; }

        private List<KeyValuePair<string, string>> _propertyHeaders;

        /// <summary>
        /// Gets the headers which were applied using properties
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> PropertyHeaders => this._propertyHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>();

        private List<KeyValuePair<string, string>> _methodHeaders;

        /// <summary>
        /// Gets the headers which were applied to the method being called
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> MethodHeaders => this._methodHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>();

        private List<KeyValuePair<string, string>> _headerParams;

        /// <summary>
        /// Gets the headers which were passed to the method as parameters
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> HeaderParams => this._headerParams ?? Enumerable.Empty<KeyValuePair<string, string>>();

        /// <summary>
        /// Gets information the [Body] method parameter, if it exists
        /// </summary>
        public BodyParameterInfo BodyParameterInfo { get; private set; }

        /// <summary>
        /// Gets the MethodInfo of the interface method which was invoked
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestInfo"/> class
        /// </summary>
        /// <param name="method">HttpMethod to use when making the request</param>
        /// <param name="path">Relative path to request</param>
        public RequestInfo(HttpMethod method, string path) : this(method, path, null)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestInfo"/> class
        /// </summary>
        /// <param name="method">HttpMethod to use when making the request</param>
        /// <param name="path">Relative path to request</param>
        /// <param name="methodInfo">MethodInfo for the method which was invoked</param>
        public RequestInfo(HttpMethod method, string path, MethodInfo methodInfo)
        {
            this.Method = method;
            this.Path = path;
            this.MethodInfo = methodInfo;
        }

        /// <summary>
        /// Add a query parameter
        /// </summary>
        /// <remarks>value may be an IEnumerable, in which case each value is added separately</remarks>
        /// <typeparam name="T">Type of the value to add</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">
        /// Format string to be passed to the custom serializer (if serializationMethod is <see cref="QuerySerializationMethod.Serialized"/>),
        /// or to the value's ToString() method (if serializationMethod is <see cref="QuerySerializationMethod.ToString"/> and value implements
        /// <see cref="IFormattable"/>)
        /// </param>
        public void AddQueryParameter<T>(QuerySerializationMethod serializationMethod, string name, T value, string format = null)
        {
            if (this._queryParams == null)
                this._queryParams = new List<QueryParameterInfo>();

            this._queryParams.Add(new QueryParameterInfo<T>(serializationMethod, name, value, format));
        }

        /// <summary>
        /// Add a collection of query parameter values under the same name
        /// </summary>
        /// <typeparam name="T">Type of the value to add</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="name">Name of the name/values pair</param>
        /// <param name="values">Values of the name/values pairs</param>
        /// <param name="format">
        /// Format string to be passed to the custom serializer (if serializationMethod is <see cref="QuerySerializationMethod.Serialized"/>),
        /// or to the value's ToString() method (if serializationMethod is <see cref="QuerySerializationMethod.ToString"/> and value implements
        /// <see cref="IFormattable"/>)
        /// </param>
        public void AddQueryCollectionParameter<T>(QuerySerializationMethod serializationMethod, string name, IEnumerable<T> values, string format = null)
        {
            if (this._queryParams == null)
                this._queryParams = new List<QueryParameterInfo>();

            this._queryParams.Add(new QueryCollectionParameterInfo<T>(serializationMethod, name, values, format));
        }

        /// <summary>
        /// Add a query map to the query parameters list, where the type of value is scalar
        /// </summary>
        /// <typeparam name="TKey">Type of key in the query map</typeparam>
        /// <typeparam name="TValue">Type of value in the query map</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="queryMap">Query map to add</param>
        public void AddQueryMap<TKey, TValue>(QuerySerializationMethod serializationMethod, IDictionary<TKey, TValue> queryMap)
        {
            if (queryMap == null)
                return;

            if (this._queryParams == null)
                this._queryParams = new List<QueryParameterInfo>();

            foreach (var kvp in queryMap)
            {
                if (kvp.Key == null)
                    continue;

                // Backwards compat: if it's a dictionary of object, see if it's ienumerable.
                // If it is, treat it as an ienumerable<object> (yay covariance)
                if (serializationMethod == QuerySerializationMethod.ToString &&
                    typeof(TValue) == typeof(object) &&
                    kvp.Value is IEnumerable<object> &&
                    !(kvp.Value is string))
                {
                    this._queryParams.Add(new QueryCollectionParameterInfo<object>(serializationMethod, kvp.Key.ToString(), (IEnumerable<object>)kvp.Value, format: null));
                }
                else
                {
                    this._queryParams.Add(new QueryParameterInfo<TValue>(serializationMethod, kvp.Key.ToString(), kvp.Value, format: null));
                }
            }
        }

        /// <summary>
        /// Add a query map to the query parameters list, where the type of value is enumerable
        /// </summary>
        /// <typeparam name="TKey">Type of key in the query map</typeparam>
        /// <typeparam name="TValue">Type of value in the query map</typeparam>
        /// <typeparam name="TElement">Type of element in the value</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="queryMap">Query map to add</param>
        public void AddQueryCollectionMap<TKey, TValue, TElement>(QuerySerializationMethod serializationMethod, IDictionary<TKey, TValue> queryMap) where TValue : IEnumerable<TElement>
        {
            if (queryMap == null)
                return;

            if (this._queryParams == null)
                this._queryParams = new List<QueryParameterInfo>();

            foreach (var kvp in queryMap)
            {
                if (kvp.Key != null)
                    this._queryParams.Add(new QueryCollectionParameterInfo<TElement>(serializationMethod, kvp.Key.ToString(), kvp.Value, format: null));
            }
        }

        /// <summary>
        /// Add a raw query parameter, which provides a string which is inserted verbatim into the query string
        /// </summary>
        /// <typeparam name="T">Type of the raw query parmaeter</typeparam>
        /// <param name="value">Raw query parameter</param>
        public void AddRawQueryParameter<T>(T value)
        {
            this.RawQueryParameter = new RawQueryParameterInfo<T>(value);
        }

        /// <summary>
        /// Add a path parameter: a [Path] method parameter which is used to substitute a placeholder in the path
        /// </summary>
        /// <typeparam name="T">Type of the value of the path parameter</typeparam>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format string to pass to ToString(), if the value implements <see cref="IFormattable"/></param>
        /// <param name="urlEncode">Whether or not this path parameter should be URL-encoded</param>
        public void AddPathParameter<T>(string name, T value, string format = null, bool urlEncode = true)
        {
            if (this._pathParams == null)
                this._pathParams = new List<PathParameterInfo>();

            this._pathParams.Add(new PathParameterInfo(name, value, format, urlEncode));
        }

        /// <summary>
        /// Add a path parameter from a property: a [Path] property which is used to substitute a placeholder in the path
        /// </summary>
        /// <typeparam name="T">Type of the value of the path parameter</typeparam>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format string to pass to ToString(), if the value implements <see cref="IFormattable"/></param>
        /// <param name="urlEncode">Whether or not this path parameter should be URL-encoded</param>
        public void AddPathProperty<T>(string name, T value, string format = null, bool urlEncode = true)
        {
            if (this._pathProperties == null)
                this._pathProperties = new List<PathParameterInfo>();

            this._pathProperties.Add(new PathParameterInfo(name, value, format, urlEncode));
        }

        /// <summary>
        /// Add a query parameter from a property: a [Query] property which is used to append to the query string of all requests
        /// </summary>
        /// <typeparam name="T">Type of value of the query parameter</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">
        /// Format string to be passed to the custom serializer (if serializationMethod is <see cref="QuerySerializationMethod.Serialized"/>),
        /// or to the value's ToString() method (if serializationMethod is <see cref="QuerySerializationMethod.ToString"/> and value implements
        /// <see cref="IFormattable"/>)
        /// </param>
        public void AddQueryProperty<T>(QuerySerializationMethod serializationMethod, string name, T value, string format)
        {
            if (this._queryProperties == null)
                this._queryProperties = new List<QueryParameterInfo>();

            this._queryProperties.Add(new QueryParameterInfo<T>(serializationMethod, name, value, format));
        }

        /// <summary>
        /// Add a property header
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="name">Name of the header</param>
        /// <param name="value">Value of the header</param>
        /// <param name="defaultValue">Value to use if 'value' == null</param>
        public void AddPropertyHeader<T>(string name, T value, string defaultValue)
        {
            if (this._propertyHeaders == null)
                this._propertyHeaders = new List<KeyValuePair<string, string>>();

            string stringValue = defaultValue;
            if (value != null)
                stringValue = value.ToString();
            this._propertyHeaders.Add(new KeyValuePair<string, string>(name, stringValue));
        }

        /// <summary>
        /// Add a header which is defined on the method
        /// </summary>
        /// <param name="name">Name of the header to add</param>
        /// <param name="value">Value of the header to add</param>
        public void AddMethodHeader(string name, string value)
        {
            if (this._methodHeaders == null)
                this._methodHeaders = new List<KeyValuePair<string, string>>();

            this._methodHeaders.Add(new KeyValuePair<string, string>(name, value));
        }

        /// <summary>
        /// Add a header which is defined as a [Header("foo")] parameter to the method
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="name">Name of the header (passed to the HeaderAttribute)</param>
        /// <param name="value">Value of the header (value of the parameter)</param>
        public void AddHeaderParameter<T>(string name, T value)
        {
            if (this._headerParams == null)
                this._headerParams = new List<KeyValuePair<string, string>>();

            string stringValue = null;
            if (value != null)
                stringValue = value.ToString();
            this._headerParams.Add(new KeyValuePair<string, string>(name, stringValue));
        }

        /// <summary>
        /// Set the body specified by the optional [Body] method parameter
        /// </summary>
        /// <param name="serializationMethod">Method to use to serialize the body</param>
        /// <param name="value">Body to serialize</param>
        /// <typeparam name="T">Type of the body's value</typeparam>
        public void SetBodyParameterInfo<T>(BodySerializationMethod serializationMethod, T value)
        {
            this.BodyParameterInfo = new BodyParameterInfo<T>(serializationMethod, value);
        }
    }
}
