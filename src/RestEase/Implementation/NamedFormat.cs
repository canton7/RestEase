using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RestEase.Implementation
{
    /// <summary>
    /// Represents a format string with named arguments.
    /// </summary>
    public class NamedFormat
    {
        private static readonly Regex argumentMatch = new Regex(@"\{(.+?)\}");

        private string _format;

        /// <summary>
        /// Create a new instance of the <see cref="NamedFormat"/> class. 
        /// </summary>
        /// <param name="format">A format string.</param>
        public NamedFormat(string format)
        {
            _format = format;
        }

        /// <summary>
        /// Gets all named arguments in the string;
        /// </summary>
        /// <returns>A list of named arguments in the string.</returns>
        public IEnumerable<NamedArgument> GetArguments()
        {
            return argumentMatch.Matches(_format).Cast<Match>().Select(ConvertToArgument);
        }

        /// <summary>
        /// Allows the arguments to be replaced with the providde values. 
        /// </summary>
        /// <param name="values">The argument values that will be applied.</param>
        /// <returns>A builder to configure the substitution options.</returns>
        public ReplaceBuilder ReplaceWith(Dictionary<string, object> values)
        {
            return new ReplaceBuilder(this, values);
        }

        private string ForEachArgument(Func<NamedArgument, string> iterator)
        {
            var matchEvaluator = new MatchEvaluator(m => iterator(ConvertToArgument(m)));
            return argumentMatch.Replace(_format, matchEvaluator);
        }

        private NamedArgument ConvertToArgument(Match match)
        {
            string value = match.Groups[1].Value;
            string[] parts = value.Split(new[] { ':' }, 2);
            if (parts.Length > 1)
            {
                return new NamedArgument(parts[0], parts[1]);
            }

            return new NamedArgument(value);
        }
        
        /// <summary>
        /// A named argument in a format.
        /// </summary>
        public class NamedArgument
        {
            internal NamedArgument(string name)
                : this(name, null)
            {
            }

            internal NamedArgument(string name, string format)
            {
                this.Name = name;
                this.Format = format;
            }

            /// <summary>
            /// Gets the name of the argument.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the format of the argument or null if there is no format.
            /// </summary>
            public string Format { get; }

            /// <summary>
            /// Determines if the argument has provided a foramt.
            /// </summary>
            /// <returns>True if argument as a format; otherwise false.</returns>
            public bool HasFormat()
            {
                return !string.IsNullOrEmpty(Format);
            }
        }

        /// <summary>
        /// A builder to configure the argument substition options.
        /// </summary>
        public class ReplaceBuilder
        {
            private NamedFormat format;
            private Dictionary<string, object> values;
            private Func<string, string> transform;

            internal ReplaceBuilder(NamedFormat format, Dictionary<string, object> values)
            {
                this.format = format;
                this.values = values;
            }

            /// <summary>
            /// Transforms the string value before applied to the format.
            /// </summary>
            /// <param name="transform">The transform to execute.</param>
            /// <returns>A builder to configure the substitution options.</returns>
            public ReplaceBuilder WithTransform(Func<string, string> transform)
            {
                this.transform = transform;
                return this;
            }

            /// <summary>
            /// Replaces all arguments in the format with the provided value.
            /// </summary>
            /// <returns>A new string with all substitutions.</returns>
            public string StringValue()
            {
                return format.ForEachArgument(ReplaceWithValue);
            }

            private string ReplaceWithValue(NamedArgument arg)
            {
                string formattedValue = string.Empty;

                object value;
                bool success = this.values.TryGetValue(arg.Name, out value);
                if (success)
                {
                    IFormattable formattable = value as IFormattable;
                    if (formattable != null && arg.HasFormat())
                    {
                        formattedValue = formattable.ToString(arg.Format, CultureInfo.InvariantCulture);
                    }
                    else if (value != null)
                    {
                        formattedValue = value.ToString();
                    }
                }

                if (this.transform != null)
                {
                    formattedValue = this.transform(formattedValue);
                }

                return formattedValue;
            }
        }
    }
}