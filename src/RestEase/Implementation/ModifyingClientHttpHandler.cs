using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    /// <summary>
    /// HttpClientHandler which uses a delegate to modify 
    /// </summary>
    public class ModifyingClientHttpHandler : HttpClientHandler
    {
        private readonly RequestModifier requestModifier;

        /// <summary>
        /// Initialises a new instance of the <see cref="ModifyingClientHttpHandler"/> class,
        /// using the given delegate to modify requests
        /// </summary>
        /// <param name="requestModifier">Delegate to use to modify requests</param>
        public ModifyingClientHttpHandler(RequestModifier requestModifier)
        {
            if (requestModifier == null)
                throw new ArgumentNullException("requestInterceptor");

            this.requestModifier = requestModifier;
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
            await this.requestModifier(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
