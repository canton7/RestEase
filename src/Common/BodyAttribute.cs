using System;

namespace RestEase
{
    /// <summary>
    /// Attribute specifying that this parameter should be interpreted as the request body
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class BodyAttribute : Attribute
    {
        /// <summary>
        /// Gets the serialization method to use. Defaults to BodySerializationMethod.Serialized
        /// </summary>
        public BodySerializationMethod SerializationMethod { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BodyAttribute"/> class
        /// </summary>
        public BodyAttribute()
            : this(BodySerializationMethod.Default)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="BodyAttribute"/> class, using the given body serialization method
        /// </summary>
        /// <param name="serializationMethod">Serialization method to use when serializing the body object</param>
        public BodyAttribute(BodySerializationMethod serializationMethod)
        {
            this.SerializationMethod = serializationMethod;
        }
    }
}
