using System;

namespace RestEase
{
    /// <summary>
    /// Marks a parameter as being the method's Query Map
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class QueryMapAttribute : Attribute
    {
        /// <summary>
        /// Gets and sets the serialization method to use to serialize the value. Defaults to QuerySerializationMethod.ToString
        /// </summary>
        public QuerySerializationMethod SerializationMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMapAttribute"/> class
        /// </summary>
        public QueryMapAttribute()
            : this(QuerySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMapAttribute"/> with the given serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use to serialize the value</param>
        public QueryMapAttribute(QuerySerializationMethod serializationMethod)
        {
            this.SerializationMethod = serializationMethod;
        }
    }
}
