using System;
using System.Net.Http;

namespace RestEase
{
    /// <summary>
    /// Response containing both the HttpResponseMessage and deserialized response
    /// </summary>
    /// <typeparam name="T">Type of deserialized response</typeparam>
    public class Response<T> : IDisposable
    {
        private readonly Func<T> contentDeserializer;
        private bool contentDeserialized;
        private T deserializedContent = default!;

        /// <summary>
        /// Gets the raw HttpResponseMessage
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the string content of the response, if there is a response
        /// </summary>
        public string? StringContent { get; private set; }

        /// <summary>
        /// Gets the deserialized response
        /// </summary>
        /// <returns>The deserialized content</returns>
        public T GetContent()
        {
            if (!this.contentDeserialized)
            {
                this.deserializedContent = this.contentDeserializer();
                this.contentDeserialized = true;
            }

            return this.deserializedContent;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Response{T}"/> class
        /// </summary>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">HttpResponseMessage received</param>
        /// <param name="contentDeserializer">Func which will deserialize the content into a T</param>
        public Response(string? content, HttpResponseMessage response, Func<T> contentDeserializer)
        {
            this.StringContent = content;
            this.ResponseMessage = response;
            this.contentDeserializer = contentDeserializer;
        }

        /// <summary>
        /// Disposes the underlying <see cref="HttpResponseMessage"/>
        /// </summary>
        public void Dispose()
        {
            this.ResponseMessage.Dispose();
        }
    }
}
