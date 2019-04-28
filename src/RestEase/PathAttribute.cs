using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as able to substitute a placeholder in this method's path
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PathAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the optional name of the placeholder. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the serialization method to use to serialize the value. Defaults to PathSerializationMethod.ToString
        /// </summary>
        public PathSerializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Gets or sets the format string to pass to the value's ToString() method, if the value implements <see cref="IFormattable"/>
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this path parameter should be URL-encoded. Defaults to true.
        /// </summary>
        public bool UrlEncode { get; set; } = true;

        /// <summary>
        /// Initialises a new instance of the <see cref="PathAttribute"/> class
        /// </summary>
        public PathAttribute()
            : this(PathSerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathAttribute"/> class, with the given serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public PathAttribute(PathSerializationMethod serializationMethod)
        {
            // Don't set this.Name
            this.SerializationMethod = serializationMethod;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Placeholder in the path to replace</param>
        public PathAttribute(string name)
            : this(name, PathSerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PathAttribute"/> class, with the given name and serialization method
        /// </summary>
        /// <param name="name">Placeholder in the path to replace</param>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public PathAttribute(string name, PathSerializationMethod serializationMethod)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
        }
    }
}
