using System;
using System.Text.RegularExpressions;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper methods to turn a value into a string
    /// </summary>
    public static class ToStringHelper
    {
        private static readonly Regex stringFormatRegex;

        static ToStringHelper()
        {
            // We know that format strings:
            // 1. Don't support a single { or }, unless they're the outermost { or } in a placeholder
            // 2. Placeholders start with a digit
            // 3. Placeholders have an optional alignment, and an optional format string.
            // 3a. The alignment can be a positive or negative integer ('+' is not allowed)
            // 3b. The format string cannot have a single { or }

            string noSingleBraces = @"(?:[^{}]|{{|}})";
            string placeholder = @"{\d+(?:,-?\d+)?(?::" + noSingleBraces + @"*)?}";
            string regex = $@"^{noSingleBraces}*(?:{placeholder}{noSingleBraces}*)+$";
            stringFormatRegex = new Regex(regex);
        }

        /// <summary>
        /// Turns the given value into a string, passing it into <see cref="string.Format(IFormatProvider, string, object[])"/> if it
        /// looks like a format string, otherwise using its <see cref="IFormattable"/> implementation if possible
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">Value to turn into a string</param>
        /// <param name="format">Format parameter, see description</param>
        /// <param name="formatProvider">Format provider to pass to <see cref="IFormattable.ToString(string, IFormatProvider)"/></param>
        /// <returns>String version of the input value</returns>
        public static string? ToString<T>(T value, string? format, IFormatProvider? formatProvider)
        {
            // If it looks like it's a ToString placeholder, use ToString
            if (format != null && stringFormatRegex.IsMatch(format))
                return string.Format(formatProvider, format, value);
            if (value is IFormattable formattable)
                return formattable.ToString(format, formatProvider);
            else
                return value?.ToString();
        }
    }
}
