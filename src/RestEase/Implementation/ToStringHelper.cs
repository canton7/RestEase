using System;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper methods to turn a value into a string
    /// </summary>
    public static class ToStringHelper
    {
        /// <summary>
        /// Turns the given value into a string, using its <see cref="IFormattable"/> implementation if possibule
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">Value to turn into a string</param>
        /// <param name="format">Format parameter to pass to <see cref="IFormattable.ToString(string, IFormatProvider)"/></param>
        /// <param name="formatProvider">Format provider to pass to <see cref="IFormattable.ToString(string, IFormatProvider)"/></param>
        /// <returns>String version of the input value</returns>
        public static string ToString<T>(T value, string format, IFormatProvider formatProvider)
        {
            if (value is IFormattable formattable)
                return formattable.ToString(format, formatProvider);
            else
                return value?.ToString();
        }
    }
}
