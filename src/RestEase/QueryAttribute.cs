using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being a query param
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class QueryAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the query param. Will use the parameter name if null
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the serialization method to use to serialize the value. Defaults to QuerySerializationMethod.ToString
        /// </summary>
        public QuerySerializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class
        /// </summary>
        public QueryAttribute()
            : this(null, QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        public QueryAttribute(string name)
            : this(name, QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(QuerySerializationMethod serializationMethod)
            : this(null, serializationMethod)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given name and serialization method
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryAttribute(string name, QuerySerializationMethod serializationMethod)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
        }
    }
}
