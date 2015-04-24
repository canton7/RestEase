using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    public class InterceptingHttpClientHandler : HttpClientHandler
    {
        private readonly RequestInterceptor requestInterceptor;

        public InterceptingHttpClientHandler(RequestInterceptor requestInterceptor)
        {
            if (requestInterceptor == null)
                throw new ArgumentNullException("requestInterceptor");

            this.requestInterceptor = requestInterceptor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this.requestInterceptor(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
