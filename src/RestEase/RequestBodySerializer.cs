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
        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the body to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        [Obsolete("Override SerializeBody<T>(T body, RequestBodySerializerInfo info) instead")]
        public virtual HttpContent SerializeBody<T>(T body)
        {
            throw new NotImplementedException("You must override and implement SerializeBody<T>(T body, RequestBodySerializerInfo info)");
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
            // By default, call the legacy SerializeBody<T>(T body) method
            return this.SerializeBody(body);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
