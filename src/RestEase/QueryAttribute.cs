using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a query param
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class QueryAttribute : Attribute
    {
        private string _name;

        /// <summary>
        /// Gets or sets the name of the query param. Will use the parameter / property name if unset.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                this.HasName = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has set the name attribute
        /// </summary>
        public bool HasName { get; private set; }

        /// <summary>
        /// Gets the serialization method to use to serialize the value. Defaults to QuerySerializationMethod.ToString
        /// </summary>
        public QuerySerializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Gets or sets a format string to be passed to the custom serializer (if serializationMethod is
        /// <see cref="QuerySerializationMethod.Serialized"/>), or to the value's ToString() method (if serializationMethod
        /// is <see cref="QuerySerializationMethod.ToString"/> and value implements <see cref="IFormattable"/>)
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class
        /// </summary>
        public QueryAttribute()
            : this(null, false, QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        public QueryAttribute(string name)
            : this(name, true, QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(QuerySerializationMethod serializationMethod)
            : this(null, false, serializationMethod)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given name and serialization method
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(string name, QuerySerializationMethod serializationMethod)
            : this(name, true, serializationMethod)
        {
        }

        private QueryAttribute(string name, bool hasName, QuerySerializationMethod serializationMethod)
        {
            this.Name = name;
            this.HasName = hasName;
            this.SerializationMethod = serializationMethod;
        }
    }
}
