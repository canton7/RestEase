using System;

namespace RestEase
{
    /// <summary>
    /// Helper used to create a properly encoded query string for a request
    /// </summary>
    public abstract class QueryStringBuilder
    {
        /// <summary>
        /// Override this method to return a suitably escaped query string
        /// </summary>
        /// <param name="info">Information about the request</param>
        /// <returns>The escaped query string</returns>
        public abstract string Build(QueryStringBuilderInfo info);
    }
}
