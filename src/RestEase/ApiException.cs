using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Exception thrown when a bad API response is received
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code received
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the ReasonPhrase associated with the response
        /// </summary>
        public string ReasonPhrase { get; private set; }

        /// <summary>
        /// Gets the headers associated with the response
        /// </summary>
        public HttpResponseHeaders Headers { get; private set; }

        /// <summary>
        /// Gets the content headers associated with the response
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; private set; }

        /// <summary>
        /// Gets the content associated with the response, if any
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets a value indicating whether any content is associated with the response
        /// </summary>
        public bool HasContent
        {
            get { return !String.IsNullOrWhiteSpace(this.Content); }
        }

        internal ApiException(
            HttpStatusCode statusCode,
            string reasonPhrase,
            HttpResponseHeaders headers,
            HttpContentHeaders contentHeaders,
            string content)
            : base(String.Format("Response status code does not indicate success: {0} ({1}).", (int)statusCode, reasonPhrase))
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
            this.Headers = headers;
            this.ContentHeaders = contentHeaders;
            this.Content = content;
        }

        internal static async Task<ApiException> CreateAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
                return new ApiException(response.StatusCode, response.ReasonPhrase, response.Headers, null, null);

            HttpContentHeaders contentHeaders = null;
            string contentString = null;

            try
            {
                contentHeaders = response.Content.Headers;
                using (var content = response.Content)
                {
                    contentString = await content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch
            { } // Don't want to hide the original exception with a new one

            return new ApiException(response.StatusCode, response.ReasonPhrase, response.Headers, contentHeaders, contentString);
        }
    }
}
