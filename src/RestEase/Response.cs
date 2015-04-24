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
        /// Raw HttpResponseMessage
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; private set; }

        /// <summary>
        /// Deserialized response
        /// </summary>
        public T Content { get; private set; }

        public Response(HttpResponseMessage response, T content)
        {
            this.ResponseMessage = response;
            this.Content = content;
        }
    }
}
