using System;
using System.Net.Http;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired request body
    /// </summary>
    public abstract class BodyParameterInfo
    {
        /// <summary>
        /// Gets or sets the method to use to serialize the body
        /// </summary>
        public BodySerializationMethod SerializationMethod { get; }

        /// <summary>
        /// Gets the body to serialize, as an object
        /// </summary>
        public object? ObjectValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the body</param>
        /// <param name="objectValue">Body to serialize, as an object</param>
        protected BodyParameterInfo(BodySerializationMethod serializationMethod, object? objectValue)
        {
            this.SerializationMethod = serializationMethod;
            this.ObjectValue = objectValue;
        }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use if the value implements <see cref="IFormattable"/></param>
        /// <returns>Serialized value</returns>
        public abstract HttpContent? SerializeValue(RequestBodySerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider);
    }

    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Class containing information about a desired request body
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class BodyParameterInfo<T> : BodyParameterInfo
    {
        /// <summary>
        /// Gets the body to serialize
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyParameterInfo{T}"/> class
        /// </summary>
        /// <param name="serializationMethod">Method to use the serialize the body</param>
        /// <param name="value">Body to serialize</param>
        public BodyParameterInfo(BodySerializationMethod serializationMethod, T value)
            : base(serializationMethod, value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Serialize the (typed) value using the given serializer
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> to use if the value implements <see cref="IFormattable"/></param>
        /// <returns>Serialized value</returns>
        public override HttpContent? SerializeValue(RequestBodySerializer serializer, IRequestInfo requestInfo, IFormatProvider? formatProvider)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            return serializer.SerializeBody(this.Value, new RequestBodySerializerInfo(requestInfo, formatProvider));
        }
    }
}
