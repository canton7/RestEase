using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestEase
{
    internal class Requester
    {
        private readonly HttpClient httpClient;
        public IResponseDeserializer ResponseDeserializer { get; set; }

        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(RequestInfo requestInfo)
        {
            var uriBuilder = new UriBuilder(requestInfo.Path);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var param in requestInfo.Parameters)
            {
                query[param.Name] = param.Value;
            }
            uriBuilder.Query = query.ToString();

            var message = new HttpRequestMessage()
            {
                Method = requestInfo.Method,
                RequestUri = uriBuilder.Uri,
            };

            var response = await this.httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, requestInfo.CancellationToken);

            if (!response.IsSuccessStatusCode)
                throw await ApiException.CreateAsync(response);

            return response;
        }

        public async Task RequestVoidAsync(RequestInfo requestInfo)
        {
            await this.SendRequestAsync(requestInfo);
        }

        public async Task<T> RequestAsync<T>(RequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo);
            T deserializedResponse = await this.ResponseDeserializer.ReadAndDeserialize<T>(response, requestInfo.CancellationToken);
            return deserializedResponse;
        }
    }
}
