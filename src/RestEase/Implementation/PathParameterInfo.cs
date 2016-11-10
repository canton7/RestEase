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
        /// Initialises a new instance of the <see cref="PathParameterInfo"/> Structure
        /// </summary>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format parameter to pass to ToString if value implements <see cref="IFormattable"/></param>
        public PathParameterInfo(string name, object value, string format)
        {
            this.name = name;
            this.value = value;
            this.format = format;
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public KeyValuePair<string, string> SerializeToString()
        {
            string stringValue;

            var formattable = this.value as IFormattable;
            if (formattable != null)
                stringValue = formattable.ToString(this.format, null);
            else
                stringValue = this.value?.ToString();

            return new KeyValuePair<string, string>(this.name, stringValue);
        }
    }
}
