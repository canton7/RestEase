using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;

namespace RestEase
{
    /// <summary>
    /// Called by the generated REST API implementation, this knows how to invoke the API and return a suitable response
    /// </summary>
    public interface IRequester : IDisposable
    {
        /// <summary>
        /// Invoked when the API interface method being called returns a Task
        /// </summary>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        Task RequestVoidAsync(IRequestInfo requestInfo);

        /// <summary>
        /// Invoked when the API interface method being called returns a Task{T}
        /// </summary>
        /// <typeparam name="T">Type of response object expected by the caller</typeparam>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        Task<T> RequestAsync<T>(IRequestInfo requestInfo);

        /// <summary>
        /// Invoked when the API interface method being called returns a Task{HttpResponseMessage}
        /// </summary>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        Task<HttpResponseMessage> RequestWithResponseMessageAsync(IRequestInfo requestInfo);

        /// <summary>
        /// Invoked when the API interface method being called returns a Task{Response{T}}
        /// </summary>
        /// <typeparam name="T">Type of response object expected by the caller</typeparam>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        Task<Response<T>> RequestWithResponseAsync<T>(IRequestInfo requestInfo);

        /// <summary>
        /// Invoked when the API interface method being called returns a Task{Stream}
        /// </summary>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        Task<Stream?> RequestStreamAsync(IRequestInfo requestInfo);
    }
}
