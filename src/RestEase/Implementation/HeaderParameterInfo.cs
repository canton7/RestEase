using System;
using System.Collections.Generic;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired header parameter
    /// </summary>
    public abstract class HeaderParameterInfo
    {
        /// <summary>
        /// Serialize the value into a name -> value pair using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public abstract KeyValuePair<string, string?> SerializeToString(IFormatProvider? formatProvider);
    }

    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired header parameter
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class HeaderParameterInfo<T> : HeaderParameterInfo
    {
        private readonly string name;
        private readonly T value;
        private readonly string? defaultValue;
        private readonly string? format;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderParameterInfo{T}"/> class
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="value">Value of the header</param>
        /// <param name="defaultValue">Default value of the header, used if <paramref name="value"/> is null</param>
        /// <param name="format"></param>
        public HeaderParameterInfo(string name, T value, string? defaultValue, string? format)
        {
            this.name = name;
            this.value = value;
            this.defaultValue = defaultValue;
            this.format = format;
        }

        /// <summary>
        /// Serialize the value into a name -> value pair using its ToString method
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> given to the <see cref="Requester"/>, if any</param>
        /// <returns>Serialized value</returns>
        public override KeyValuePair<string, string?> SerializeToString(IFormatProvider? formatProvider)
        {
            string? value = this.defaultValue;
            if (this.value != null)
                value = ToStringHelper.ToString(this.value, this.format, formatProvider);
            return new KeyValuePair<string, string?>(this.name, value);
        }
    }
}
