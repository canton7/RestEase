using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Response containing both the HttpResponseMessage and deserialized response
    /// </summary>
    /// <typeparam name="T">Type of deserialized response</typeparam>
    public class Response<T>
    {
        private readonly Lazy<Task<T>> contentDeserializer;

        /// <summary>
        /// Gets the raw HttpResponseMessage
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; private set; }
        
        /// <summary>
        /// Gets the deserialized response
        /// </summary>
        /// <returns>The deserialized content</returns>
        public Task<T> GetContent()
        {
            return this.contentDeserializer.Value;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Response{T}"/> class
        /// </summary>
        /// <param name="response">HttpResponseMessage received</param>
        /// <param name="contentDeserializer">Func which will deserialize the content into a T</param>
        public Response(HttpResponseMessage response, Func<Task<T>> contentDeserializer)
        {
            this.ResponseMessage = response;
            this.contentDeserializer = new Lazy<Task<T>>(contentDeserializer);
        }
    }
}
