using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information to construct a request from.
    /// An instance of this is created per request by the generated interface implementation
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>
        /// Gets the HttpMethod which should be used to make the request
        /// </summary>
        HttpMethod Method { get; }

        /// <summary>
        /// Gets the relative path to the resource to request
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the CancellationToken used to cancel the request
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a value indicating whether to suppress the exception on invalid status codes
        /// </summary>
        bool AllowAnyStatusCode { get; }

        /// <summary>
        /// Gets the query parameters to append to the request URI
        /// </summary>
        IReadOnlyList<QueryParameterInfo> QueryParams { get; }

        /// <summary>
        /// Gets the query map, if specified. Must be an IDictionary or IDictionary{TKey, TValue}
        /// </summary>
        object QueryMap { get; }

        /// <summary>
        /// Gets the parameters which should be substituted into placeholders in the Path
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> PathParams { get; }

        /// <summary>
        /// Gets the headers which were applied to the interface
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> ClassHeaders { get; }

        /// <summary>
        /// Gets the headers which were applied using properties
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> PropertyHeaders { get; }

        /// <summary>
        /// Gets the headers which were applied to the method being called
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> MethodHeaders { get; }

        /// <summary>
        /// Gets the headers which were passed to the method as parameters
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> HeaderParams { get; }

        /// <summary>
        /// Gets information the [Body] method parameter, if it exists
        /// </summary>
        BodyParameterInfo BodyParameterInfo { get; }
    }
}
