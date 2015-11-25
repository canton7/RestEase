using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
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
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// Gets the relative path to the resource to request
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets the CancellationToken used to cancel the request
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress the exception on invalid status codes
        /// </summary>
        public bool AllowAnyStatusCode { get; set; }

        private readonly List<QueryParameterInfo> _queryParams;

        /// <summary>
        /// Gets the query parameters to append to the request URI
        /// </summary>
        public IReadOnlyList<QueryParameterInfo> QueryParams
        {
            get { return this._queryParams; }
        }

        private readonly List<KeyValuePair<string, string>> _pathParams;

        /// <summary>
        /// Gets the parameters which should be substituted into placeholders in the Path
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> PathParams
        {
            get { return this._pathParams; }
        }

        /// <summary>
        /// Gets or sets the headers which were applied to the interface
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> ClassHeaders { get; set; }

        private readonly List<KeyValuePair<string, string>> _propertyHeaders;

        /// <summary>
        /// Gets the headers which were applied using properties
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> PropertyHeaders
        {
            get { return this._propertyHeaders; }
        }

        private readonly List<KeyValuePair<string, string>> _methodHeaders;

        /// <summary>
        /// Gets the headers which were applied to the method being called
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> MethodHeaders
        {
            get { return this._methodHeaders; }
        }

        private readonly List<KeyValuePair<string, string>> _headerParams;

        /// <summary>
        /// Gets the headers which were passed to the method as parameters
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> HeaderParams
        {
            get { return this._headerParams; }
        }

        /// <summary>
        /// Gets information the [Body] method parameter, if it exists
        /// </summary>
        public BodyParameterInfo BodyParameterInfo { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RequestInfo"/> class
        /// </summary>
        /// <param name="method">HttpMethod to use when making the request</param>
        /// <param name="path">Relative path to request</param>
        public RequestInfo(HttpMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
            this.CancellationToken = CancellationToken.None;

            this._queryParams = new List<QueryParameterInfo>();
            this._pathParams = new List<KeyValuePair<string, string>>();
            this._methodHeaders = new List<KeyValuePair<string, string>>();
            this._propertyHeaders = new List<KeyValuePair<string, string>>();
            this._headerParams = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Add a query parameter
        /// </summary>
        /// <remarks>value may be an IEnumerable, in which case each value is added separately</remarks>
        /// <typeparam name="T">Type of the value to add</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        public void AddQueryParameter<T>(QuerySerializationMethod serializationMethod, string name, T value)
        {
            this._queryParams.Add(new QueryParameterInfo<T>(serializationMethod, name, value));
        }

        /// <summary>
        /// Add a collection of query parameter values under the same name
        /// </summary>
        /// <typeparam name="T">Type of the value to add</typeparam>
        /// <param name="serializationMethod">Method to use to serialize the value</param>
        /// <param name="name">Name of the name/values pair</param>
        /// <param name="values">Values of the name/values pairs</param>
        public void AddQueryCollectionParameter<T>(QuerySerializationMethod serializationMethod, string name, IEnumerable<T> values)
        {
            this._queryParams.Add(new QueryCollectionParameterInfo<T>(serializationMethod, name, values));
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
                    this._queryParams.Add(new QueryCollectionParameterInfo<object>(serializationMethod, kvp.Key.ToString(), (IEnumerable<object>)kvp.Value));
                }
                else
                {
                    this._queryParams.Add(new QueryParameterInfo<TValue>(serializationMethod, kvp.Key.ToString(), kvp.Value));
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

            foreach (var kvp in queryMap)
            {
                if (kvp.Key != null)
                    this._queryParams.Add(new QueryCollectionParameterInfo<TElement>(serializationMethod, kvp.Key.ToString(), kvp.Value));
            }
        }

        /// <summary>
        /// Add a path parameter: a [Path] method parameter which is used to substitute a placeholder in the path
        /// </summary>
        /// <typeparam name="T">Type of the value of the path parameter</typeparam>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        public void AddPathParameter<T>(string name, T value)
        {
            string stringValue = null;
            if (value != null)
                stringValue = value.ToString();
            this._pathParams.Add(new KeyValuePair<string, string>(name, stringValue));
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
