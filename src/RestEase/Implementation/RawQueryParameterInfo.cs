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
        /// <returns>Serialized value</returns>
        public abstract string SerializeToString();
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

        /// <summary>
        /// Serialize the value into a string
        /// </summary>
        /// <returns>Serialized value</returns>
        public override string SerializeToString()
        {
            if (this.value == null)
                return String.Empty;

            return this.value.ToString();
        }
    }
}
