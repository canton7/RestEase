using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RestEase
{
    /// <summary>
    /// Exception thrown when a bad API response is received
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Gets the method used to make the request which failed
        /// </summary>
        public HttpMethod RequestMethod { get; }

        /// <summary>
        /// Gets the URI to which the request which failed wras made
        /// </summary>
        public Uri RequestUri { get; }

        /// <summary>
        /// Gets the HTTP status code received
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the ReasonPhrase associated with the response
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// Gets the headers associated with the response
        /// </summary>
        public HttpResponseHeaders Headers { get; }

        /// <summary>
        /// Gets the content headers associated with the response
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; }

        /// <summary>
        /// Gets the content associated with the response, if any
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets a value indicating whether any content is associated with the response
        /// </summary>
        public bool HasContent
        {
            get { return !String.IsNullOrWhiteSpace(this.Content); }
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiException"/> class with the given <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="request">Request which triggered the exception</param>
        /// <param name="response"><see cref="HttpResponseMessage"/> provided by the <see cref="HttpClient"/></param>
        /// <param name="contentString">String content, as read from <see cref="HttpContent.ReadAsStringAsync"/>, if there is a response content</param>
        public ApiException(HttpRequestMessage request, HttpResponseMessage response, string contentString)
            : this(request.Method,
                  request.RequestUri,
                  response.StatusCode,
                  response.ReasonPhrase,
                  response.Headers,
                  response.Content?.Headers,
                  contentString)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiException"/> class with the given components
        /// </summary>
        /// <param name="requestMethod"><see cref="HttpMethod"/> used to make the request which failed</param>
        /// <param name="requestUri"><see cref="Uri"/> to which the request which failed was made</param>
        /// <param name="statusCode"><see cref="HttpStatusCode"/> returned by the endpoint</param>
        /// <param name="reasonPhrase"><see cref="HttpResponseMessage.ReasonPhrase"/> provided by <see cref="HttpClient"/></param>
        /// <param name="headers"><see cref="HttpResponseHeaders"/>s associated with the response</param>
        /// <param name="contentHeaders"><see cref="HttpContentHeaders"/> associated with the response content, if there is a response content</param>
        /// <param name="contentString">String content, as read from <see cref="HttpContent.ReadAsStringAsync"/>, if there is a response content</param>
        public ApiException(
            HttpMethod requestMethod,
            Uri requestUri,
            HttpStatusCode statusCode,
            string reasonPhrase,
            HttpResponseHeaders headers,
            HttpContentHeaders contentHeaders,
            string contentString)
            : base($"{requestMethod} \"{requestUri}\" failed because response status code does not indicate success: {(int)statusCode} ({reasonPhrase}).")
        {
            this.RequestMethod = requestMethod;
            this.Data[nameof(this.RequestMethod)] = requestMethod.Method;

            this.RequestUri = requestUri;
            this.Data[nameof(this.RequestUri)] = requestUri;

            this.StatusCode = statusCode;
            this.Data[nameof(this.StatusCode)] = statusCode;

            this.ReasonPhrase = reasonPhrase;
            this.Data[nameof(this.ReasonPhrase)] = reasonPhrase;

            this.Headers = headers;
            this.ContentHeaders = contentHeaders;
            this.Content = contentString;
        }

        /// <summary>
        /// Create a new <see cref="ApiException"/>, by reading the response asynchronously content as a string
        /// </summary>
        /// <param name="request">Request which triggered the exception</param>
        /// <param name="response">Response received from the <see cref="HttpClient"/></param>
        /// <returns>A new <see cref="ApiException"/> created from the <see cref="HttpResponseMessage"/></returns>
        public static async Task<ApiException> CreateAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (response.Content == null)
                return new ApiException(request, response, null);

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

            return new ApiException(request, response, contentString);
        }
    }
}
