using System;
using System.Collections.Generic;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a desired query parameter
    /// </summary>
    public abstract class PathParameterInfo
    {
        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public abstract KeyValuePair<string, string> SerializeToString();
    }

    /// <summary>
    /// Class containing information about a desired path parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class PathParameterInfo<T> : PathParameterInfo
    {
        private readonly string name;
        private readonly T value;
        private readonly string format;

        /// <summary>
        /// Initialises a new instance of the <see cref="PathParameterInfo{T}"/> class
        /// </summary>
        /// <param name="name">Name of the name/value pair</param>
        /// <param name="value">Value of the name/value pair</param>
        /// <param name="format">Format parameter to pass to ToString if value implements <see cref="IFormattable"/></param>
        public PathParameterInfo(string name, T value, string format)
        {
            this.name = name;
            this.value = value;
            this.format = format;
        }

        /// <summary>
        /// Serialize the value into a collection of name -> value pairs using its ToString method
        /// </summary>
        /// <returns>Serialized value</returns>
        public override KeyValuePair<string, string> SerializeToString()
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
