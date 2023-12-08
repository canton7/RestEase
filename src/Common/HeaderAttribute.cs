using System;

namespace RestEase
{
    /// <summary>
    /// Attribute allowing interface-level, method-level, or parameter-level headers to be defined. See the docs for details
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class HeaderAttribute : Attribute
    {
        /// <summary>
        /// Gets the Name of the header
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the header, if present
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// Gets or sets the format string used to format the value, if this is used as a variable header
        /// (i.e. <see cref="Value"/> is null).
        /// </summary>
        /// <remarks>
        /// If this looks like a format string which can be passed to <see cref="string.Format(IFormatProvider, string, object[])"/>,
        /// (i.e. it contains at least one format placeholder), then this happens with the value passed as the first arg.
        /// Otherwise, if the value implements <see cref="IFormattable"/>, this is passed to the value's
        /// <see cref="IFormattable.ToString(string, IFormatProvider)"/> method. Otherwise this is ignored.
        /// Example values: "X2", "{0:X2}", "test{0}".
        /// </remarks>
        public string? Format { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="HeaderAttribute"/> class
        /// </summary>#
        /// 
        /// <param name="name">Name of the header</param>
        public HeaderAttribute(string name)
        {
            this.Name = name;
            this.Value = null;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="HeaderAttribute"/> class
        /// </summary>
        /// <param name="name">Name of the header</param>
        /// <param name="value">Value of the header</param>
        public HeaderAttribute(string name, string? value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
