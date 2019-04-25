using System;

namespace RestEase
{
    /// <summary>
    /// Specifies the default serialization methods for query parameters and request bodies (which can then be overridden)
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SerializationMethodsAttribute : Attribute
    {
        /// <summary>
        /// Gets and sets the serialization method used to serialize request bodies. Defaults to BodySerializationMethod.Serialized
        /// </summary>
        public BodySerializationMethod Body { get; set; }

        /// <summary>
        /// Gets and sets the serialization method used to serialize query parameters. Defaults to QuerySerializationMethod.ToString
        /// </summary>
        public QuerySerializationMethod Query { get; set; }

        /// <summary>
        /// Gets and sets the serialization method used to serialize path parameters. Default to PathSerializationMethod.ToString
        /// </summary>
        public PathSerializationMethod Path { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerializationMethodsAttribute"/> class
        /// </summary>
        public SerializationMethodsAttribute()
        {
            this.Body = BodySerializationMethod.Default;
            this.Query = QuerySerializationMethod.Default;
            this.Path = PathSerializationMethod.Default;
        }
    }
}
