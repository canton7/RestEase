using System.Net.Http;

namespace RestEase
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Helper which knows how to serialize a request body
    /// </summary>
    public abstract class RequestBodySerializer : IRequestBodySerializer
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Serialize the given request body
        /// </summary>
        /// <param name="body">Body to serialize</param>
        /// <typeparam name="T">Type of the body to serialize</typeparam>
        /// <returns>HttpContent to assign to the request</returns>
        public abstract HttpContent SerializeBody<T>(T body);
    }
}
