using System;
using System.Collections.Generic;

namespace RestEase.Implementation
{
    /// <summary>
    /// Structure containing information about a desired path parameter
    /// </summary>
    public struct PathParameterInfo
    {
        private readonly string name;
        private readonly object value;
        private readonly string format;

        /// <summary>
        /// Gets a value indicating whether this path parameter should be URL-encoded
        /// </summary>
        public bool UrlEncode { get; }

        /// <summary>
        /// Gets the method to use to serialize the path parameter
        /// </summary>
        public PathSerializationMethod SerializationMethod { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathParameterInfo"/> Structure
        /// </summary>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format parameter to pass to ToString if value implements <see cref="IFormattable"/></param>
        /// <param name="urlEncode">Indicates whether this parameter should be url-encoded</param>
        /// <param name="serializationMethod"></param>
        public PathParameterInfo(string name, object value, string format, bool urlEncode, PathSerializationMethod serializationMethod)
        {
            this.name = name;
            this.value = value;
            this.format = format;
            this.UrlEncode = urlEncode;
            this.SerializationMethod = serializationMethod;
        }

        public KeyValuePair<string, string> SerializeValue(RequestPathParamSerializer serializer, IRequestInfo requestInfo)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (requestInfo == null)
                throw new ArgumentNullException(nameof(requestInfo));

            return serializer.SerializePathParam(this.name, this.value, new RequestPathParamSerializerInfo(requestInfo, this.format));
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use if the value implements <see cref="IFormattable"/></param>
        /// <returns>Serialized value</returns>
        public KeyValuePair<string, string> SerializeToString(IFormatProvider formatProvider)
        {
            return new KeyValuePair<string, string>(this.name, ToStringHelper.ToString(this.value, this.format, formatProvider));
        }
    }
}
