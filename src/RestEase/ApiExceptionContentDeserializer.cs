using System.Net.Http;
using RestEase.Implementation;

namespace RestEase
{
    /// <summary>
    /// Deserializer used by <see cref="ApiException.DeserializeContent{T}"/> to deserialize its response content
    /// </summary>
    /// <remarks>This is needed to encapsulate information required by the deserializer</remarks>
    public interface IApiExceptionContentDeserializer
    {
        /// <summary>
        /// Deserialize the given content as the given type
        /// </summary>
        /// <typeparam name="T">Type to deserialize as</typeparam>
        /// <param name="content">Content to deserialize</param>
        /// <returns>Deserialized content</returns>
        T Deserialize<T>(string? content);
    }

    internal class ApiExceptionContentDeserializer : IApiExceptionContentDeserializer
    {
        private readonly Requester requester;
        private readonly HttpResponseMessage responseMessage;
        private readonly IRequestInfo requestInfo;

        public ApiExceptionContentDeserializer(Requester requester, HttpResponseMessage responseMessage, IRequestInfo requestInfo)
        {
            this.requester = requester;
            this.responseMessage = responseMessage;
            this.requestInfo = requestInfo;
        }

        public T Deserialize<T>(string? content)
        {
            return this.requester.Deserialize<T>(content, this.responseMessage, this.requestInfo);
        }
    }
}
