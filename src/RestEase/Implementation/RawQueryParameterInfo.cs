using System;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a raw query parameter
    /// </summary>
    public abstract class RawQueryParameterInfo
    {
        /// <summary>
        /// Serialize the value into a string
        /// </summary>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use if the value implements <see cref="IFormattable"/></param>
        /// <returns>Serialized value</returns>
        public abstract string SerializeToString(IFormatProvider formatProvider);
    }

    /// <summary>
    /// Class containing information about a raw query parameter
    /// </summary>
    /// <typeparam name="T">Type of value providing the raw query parameter</typeparam>
    public class RawQueryParameterInfo<T> : RawQueryParameterInfo
    {
        private readonly T value;

        /// <summary>
        /// Initialises a new instance of the <see cref="RawQueryParameterInfo{T}"/> class
        /// </summary>
        /// <param name="value">Value which provides the raw query parameter</param>
        public RawQueryParameterInfo(T value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override string SerializeToString(IFormatProvider formatProvider)
        {
            return ToStringHelper.ToString(this.value, null, formatProvider) ?? string.Empty;
        }
    }
}
