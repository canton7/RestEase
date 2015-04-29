using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Response containing both the HttpResponseMessage and deserialized response
    /// </summary>
    /// <typeparam name="T">Type of deserialized response</typeparam>
    public class Response<T>
    {
        /// <summary>
        /// Gets the raw HttpResponseMessage
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the deserialized response
        /// </summary>
        public T Content { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Response{T}"/> class
        /// </summary>
        /// <param name="response">HttpResponseMessage to retain</param>
        /// <param name="content">Deserialized response to retain</param>
        public Response(HttpResponseMessage response, T content)
        {
            this.ResponseMessage = response;
            this.Content = content;
        }
    }
}
