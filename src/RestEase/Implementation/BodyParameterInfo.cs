using RestEase.Implementation;
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
        /// Gets type of body
        /// </summary>
        public abstract Type ObjectType { get; }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <returns>Serialized value</returns>
        public abstract HttpContent SerializeValue(RequestBodySerializer serializer, IRequestInfo requestInfo);
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
        /// Gets type of body
        /// </summary>
        public override Type ObjectType { get; }

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
            this.ObjectType = typeof(T);
        }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <returns>Serialized value</returns>
        public override HttpContent SerializeValue(RequestBodySerializer serializer, IRequestInfo requestInfo)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            return serializer.SerializeBody<T>(this.Value, new RequestBodySerializerInfo(requestInfo));
        }
    }
}
