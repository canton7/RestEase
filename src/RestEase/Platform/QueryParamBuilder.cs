
using System;
using System.Collections.Generic;

namespace RestEase.Platform
{
    /// <summary>
    /// Cross-platform query parameter build utility class
    /// </summary>
    public static class QueryParamBuilder
    {
        /// <summary>
        /// Build a query string out of the given parameters
        /// </summary>
        /// <param name="initialQuery">Initial raw query string</param>
        /// <param name="parameters">Key/value pairs to add</param>
        /// <returns>Constructed query string</returns>
        public static string Build(string initialQuery, IEnumerable<KeyValuePair<string, string>> parameters)
        {
#if NETSTANDARD
            var query = initialQuery;
            foreach (var kvp in parameters)
            {
                if (kvp.Key == null)
                {
                    char separator = String.IsNullOrWhiteSpace(query) ? '?' : '&';
                    query += separator + System.Net.WebUtility.UrlEncode(kvp.Value).Replace("?", "%3f");
                }
                else
                {
                    query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(query, kvp.Key, kvp.Value);
                }
            }
            // We should use %20 before the ?, and + after it.
            return query.Replace("%20", "+");
#else
            var query = System.Web.HttpUtility.ParseQueryString(initialQuery);
            foreach (var kvp in parameters)
            {
                query.Add(kvp.Key, kvp.Value);
            }
            return query.ToString();
#endif
        }
    }
}
