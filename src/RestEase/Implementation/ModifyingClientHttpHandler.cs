using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// HttpClientHandler which uses a delegate to modify requests
    /// </summary>
    public class ModifyingClientHttpHandler : DelegatingHandler
    {
        private readonly RequestModifier requestModifier;

        /// <summary>
        /// Initialises a new instance of the <see cref="ModifyingClientHttpHandler"/> class,
        /// using the given delegate to modify requests
        /// </summary>
        /// <param name="requestModifier">Delegate to use to modify requests</param>
        public ModifyingClientHttpHandler(RequestModifier requestModifier)
        {
            this.requestModifier = requestModifier ?? throw new ArgumentNullException(nameof(requestModifier));
        }

        /// <summary>
        /// Creates an instance of System.Net.Http.HttpResponseMessage based on the information
        /// provided in the System.Net.Http.HttpRequestMessage as an operation that will
        /// not block.
        /// </summary>
        /// <param name="request">The HTTP request message</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
        /// <returns>Returns System.Threading.Tasks.Task{TResult}.The task object representing
        /// the asynchronous operation</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this.requestModifier(request, cancellationToken).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
