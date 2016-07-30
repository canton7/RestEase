
namespace RestEase.Platform
{
    public class QueryParamBuilder
    {
#if NETSTANDARD
        private string query;

        public QueryParamBuilder(string query)
        {
            this.query = query;
        }

        public void Add(string name, string value)
        {
            this.query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(this.query, name, value);
        }

        public override string ToString()
        {
            // We should use %20 before the ?, and + after it.
            return this.query.Replace("%20", "+");
        }
#else
        private readonly System.Collections.Specialized.NameValueCollection query;

        public QueryParamBuilder(string query)
        {
            this.query = System.Web.HttpUtility.ParseQueryString(query);
        }

        public void Add(string name, string value)
        {
            this.query.Add(name, value);
        }

        public override string ToString()
        {
            return this.query.ToString();
        }
#endif
    }
}
