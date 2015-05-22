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
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the header, if present
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="HeaderAttribute"/> class
        /// </summary>
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
        public HeaderAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
