using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestEase.Implementation
{
    /// <summary>
    /// Extensions for raw query parameter info
    /// </summary>
    public static class RawQueryParameterInfoExtensions
    {
        /// <summary>
        /// Serializes the raw query parameters to a query string
        /// </summary>
        /// <param name="rawQueryParameter"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static string SerializeToString(this IEnumerable<RawQueryParameterInfo> rawQueryParameter, IFormatProvider formatProvider)
        {
            if (rawQueryParameter == null || !rawQueryParameter.Any())
                return string.Empty;

            IEnumerable<string> rawQueryStrings =
                rawQueryParameter.Select(rqp => rqp.SerializeToString(formatProvider) ?? string.Empty);

            return string.Join("",rawQueryStrings);
        }
    }
}
