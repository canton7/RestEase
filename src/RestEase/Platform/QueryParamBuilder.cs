
namespace RestEase.Platform
{
    /// <summary>
    /// Cross-platform query parameter build utility class
    /// </summary>
    public class QueryParamBuilder
    {
#if NETSTANDARD
        private string query;

        /// <summary>
        /// Instantiates a new instance of the <see cref="QueryParamBuilder"/> class
        /// </summary>
        /// <param name="query">Initial query</param>
        public QueryParamBuilder(string query)
        {
            this.query = query;
        }

        /// <summary>
        /// Add a new name/value pair to the query string
        /// </summary>
        public void Add(string name, string value)
        {
            this.query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(this.query, name, value);
        }

        /// <summary>
        /// Generate the encoded query string
        /// </summary>
        public override string ToString()
        {
            // We should use %20 before the ?, and + after it.
            return this.query.Replace("%20", "+");
        }
#else
        private readonly System.Collections.Specialized.NameValueCollection query;

        /// <summary>
        /// Instantiates a new instance of the <see cref="QueryParamBuilder"/> class
        /// </summary>
        /// <param name="query">Initial query</param>
        public QueryParamBuilder(string query)
        {
            this.query = System.Web.HttpUtility.ParseQueryString(query);
        }

        /// <summary>
        /// Add a new name/value pair to the query string
        /// </summary>
        public void Add(string name, string value)
        {
            this.query.Add(name, value);
        }

        /// <summary>
        /// Generate the encoded query string
        /// </summary>
        public override string ToString()
        {
            return this.query.ToString();
        }
#endif
    }
}
