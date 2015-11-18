using System;
using System.Net.Http;

namespace RestEase.Implementation
{
    /// <summary>
    /// Class containing information about a desired request body
    /// </summary>
    public abstract class BodyParameterInfo
    {
        /// <summary>
        /// Gets or sets the method to use to serialize the body
        /// </summary>
        public BodySerializationMethod SerializationMethod { get; protected set; }

        /// <summary>
        /// Gets the body to serialize, as an object
        /// </summary>
        public object ObjectValue { get; protected set; }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public abstract HttpContent SerializeValue(IRequestBodySerializer serializer);
    }

    /// <summary>
    /// Class containing information about a desired request body
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class BodyParameterInfo<T> : BodyParameterInfo
    {
        /// <summary>
        /// Gets the body to serialize
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BodyParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the body</param>
        /// <param name="value">Body to serialize</param>
        public BodyParameterInfo(BodySerializationMethod serializationMethod, T value)
        {
            this.SerializationMethod = serializationMethod;
            this.ObjectValue = value;
            this.Value = value;
        }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <returns>Serialized value</returns>
        public override HttpContent SerializeValue(IRequestBodySerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            return serializer.SerializeBody<T>(this.Value);
        }
    }
}
