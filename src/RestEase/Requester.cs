using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestEase
{
    public class Requester : IRequester
    {
        private readonly HttpClient httpClient;
        public IResponseDeserializer ResponseDeserializer { get; set; }

        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.ResponseDeserializer = new JsonResponseDeserializer();
        }

        protected virtual async Task<HttpResponseMessage> SendRequestAsync(RequestInfo requestInfo)
        {
            var uriBuilder = new UriBuilder(requestInfo.Path);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Add(requestInfo.Parameters);
            uriBuilder.Query = query.ToString();

            var message = new HttpRequestMessage()
            {
                Method = requestInfo.Method,
                RequestUri = uriBuilder.Uri,
            };

            var response = await this.httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, requestInfo.CancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await ApiException.CreateAsync(response).ConfigureAwait(false);

            return response;
        }

        public virtual async Task RequestVoidAsync(RequestInfo requestInfo)
        {
            await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
        }

        public virtual async Task<T> RequestAsync<T>(RequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            T deserializedResponse = await this.ResponseDeserializer.ReadAndDeserialize<T>(response, requestInfo.CancellationToken).ConfigureAwait(false);
            return deserializedResponse;
        }
    }
}
