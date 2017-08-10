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
        private readonly Func<Task<T>> contentDeserializer;

        private string stringContent;
        private bool stringContentRead;

        private bool contentDeserialized;
        private T deserializedContent;

        /// <summary>
        /// Gets the raw HttpResponseMessage
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; }

        /// <summary>
        /// Gets the string content of the response
        /// </summary>
        [Obsolete("Use GetStringContentAsync instead")]
        public string StringContent => this.GetStringContentAsync().Result;

        /// <summary>
        /// Reads the content as a string
        /// </summary>
        /// <returns>The content as a string</returns>
        public async Task<string> GetStringContentAsync()
        {
            if (!this.stringContentRead)
            {
                this.stringContent = await this.ResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                this.stringContentRead = true;
            }
            return this.stringContent;
        }

        /// <summary>
        /// Gets the deserialized response
        /// </summary>
        /// <returns>The deserialized content</returns>
        [Obsolete("Use GetContentAsync instead")]
        public T GetContent()
        {
            return this.GetContentAsync().Result;
        }

        /// <summary>
        /// Gets the deserialized response
        /// </summary>
        /// <returns>The deserialized content</returns>
        public async Task<T> GetContentAsync()
        {
            if (!this.contentDeserialized)
            {
                this.deserializedContent = await this.contentDeserializer().ConfigureAwait(false);
                this.contentDeserialized = true;
            }

            return this.deserializedContent;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Response{T}"/> class
        /// </summary>
        /// <param name="response">HttpResponseMessage received</param>
        /// <param name="contentDeserializer">Func which will deserialize the content into a T</param>
        public Response(HttpResponseMessage response, Func<Task<T>> contentDeserializer)
        {
            this.ResponseMessage = response;
            this.contentDeserializer = contentDeserializer;
        }
    }
}
