namespace RestEase.Platform
{
    /// <summary>
    /// Utility providing cross-platform abstraction over url-related methods
    /// </summary>
    public static class HttpWebUtility
    {
        /// <summary>
        /// Converts a text string into a URL-encoded string.
        /// </summary>
        public static string UrlEncode(string input)
        {
#if NET40
            return System.Web.HttpUtility.UrlEncode(input);
#else
            return System.Net.WebUtility.UrlEncode(input);
#endif
        }
    }
}
