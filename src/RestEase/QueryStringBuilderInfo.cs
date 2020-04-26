using System;
using System.Collections.Generic;
using System.Linq;

namespace RestEase
{
    /// <summary>
    /// Encapsulates information provided to <see cref="QueryStringBuilder"/>
    /// </summary>
    public class QueryStringBuilderInfo
    {
        /// <summary>
        /// Gets the initial query string, present from the URI the user specified in the Get/etc parameter
        /// </summary>
        public string InitialQueryString { get; }

        /// <summary>
        /// Gets the raw query parameter, if any
        /// </summary>
        public IEnumerable<string> RawQueryParameters { get; }

        /// <summary>
        /// Obsolete. Use <see cref="RawQueryParameters"/>
        /// </summary>
        [Obsolete("Use RawQueryParameters")]
        public string RawQueryParameter => this.RawQueryParameters.FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Gets the query parameters (or an empty collection)
        /// </summary>
        public IEnumerable<KeyValuePair<string, string?>> QueryParams { get; }

        /// <summary>
        /// Gets the query properties (or an empty collection)
        /// </summary>
        public IEnumerable<KeyValuePair<string, string?>> QueryProperties { get; }

        /// <summary>
        /// Gets the RequestInfo representing the request
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// Gets the format provider, if any
        /// </summary>
        public IFormatProvider? FormatProvider { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryStringBuilderInfo"/> class
        /// </summary>
        /// <param name="initialQueryString">Initial query string, present from the URI the user specified in the Get/etc parameter</param>
        /// <param name="rawQueryParameters">The raw query parameters, if any</param>
        /// <param name="queryParams">The query parameters (or an empty collection)</param>
        /// <param name="queryProperties">The query propeorties (or an empty collection)</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider">Format provider to use to format things</param>
        public QueryStringBuilderInfo(
            string initialQueryString,
            IEnumerable<string> rawQueryParameters,
            IEnumerable<KeyValuePair<string, string?>> queryParams,
            IEnumerable<KeyValuePair<string, string?>> queryProperties,
            IRequestInfo requestInfo,
            IFormatProvider? formatProvider)
        {
            this.InitialQueryString = initialQueryString;
            this.RawQueryParameters = rawQueryParameters;
            this.QueryParams = queryParams;
            this.QueryProperties = queryProperties;
            this.RequestInfo = requestInfo;
            this.FormatProvider = formatProvider;
        }

        /// <summary>
        /// Obsolete. Use the other constructor.
        /// </summary>
        [Obsolete("Use the other constructor")]
        public QueryStringBuilderInfo(
            string initialQueryString,
            string rawQueryParameter,
            IEnumerable<KeyValuePair<string, string?>> queryParams,
            IEnumerable<KeyValuePair<string, string?>> queryProperties,
            IRequestInfo requestInfo,
            IFormatProvider? formatProvider)
            : this(initialQueryString, new[] { rawQueryParameter }, queryParams, queryProperties, requestInfo, formatProvider)
        {
        }
    }
}
