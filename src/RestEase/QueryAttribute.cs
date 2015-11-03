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

        public QuerySerialializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class
        /// </summary>
        public QueryAttribute()
            : this(null, QuerySerialializationMethod.ToString)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="QueryAttribute"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the query parameter</param>
        public QueryAttribute(string name)
            : this(name, QuerySerialializationMethod.ToString)
        { }

        public QueryAttribute(QuerySerialializationMethod serializationMethod)
            : this(null, serializationMethod)
        { }

        public QueryAttribute(string name, QuerySerialializationMethod serializationMethod)
        {
            this.Name = name;
            this.SerializationMethod = serializationMethod;
        }
    }
}
