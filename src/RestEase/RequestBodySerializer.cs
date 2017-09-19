using System;
using System.Net.Http;

namespace RestEase
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Helper which knows how to serialize a request body
    /// </summary>
    public abstract class RequestBodySerializer : IRequestBodySerializer
    {
        [Obsolete("Override SerializeBody<T>(T body, RequestBodySerializerInfo info) instead", error: true)]
        HttpContent IRequestBodySerializer.SerializeBody<T>(T body)
        {
            // This exists only so that we can assign instances of ResponseDeserializer to the IResponseDeserializer in RestClient
            throw new InvalidOperationException("This should never be called");
        }

        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <param name="info">Extra information about the request</param>
        /// <typeparam name="T">Type of the body to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        public virtual HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info)
        {
            throw new NotImplementedException($"You must override and implement SerializeBody<T>(T body, RequestBodySerializerInfo info) in {this.GetType().Name}");
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
