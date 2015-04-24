using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public IRequestBodySerializer RequestBodySerializer { get; set; }

        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.ResponseDeserializer = new JsonResponseDeserializer();
            this.RequestBodySerializer = new JsonRequestBodySerializer();
        }

        protected virtual Uri ConstructUri(RequestInfo requestInfo)
        {
            // UriBuilder insists that we provide it with an absolute URI, even though we only want a relative one...
            var uriBuilder = new UriBuilder(new Uri(new Uri("http://api"), requestInfo.Path ?? String.Empty));

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var queryParam in requestInfo.QueryParams)
            {
                query.Add(queryParam.Key, queryParam.Value);
            }
            uriBuilder.Query = query.ToString();

            return new Uri(uriBuilder.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped), UriKind.Relative);
        }

        protected virtual HttpContent ConstructContent(RequestInfo requestInfo)
        {
            if (requestInfo.BodyParameterInfo == null || requestInfo.BodyParameterInfo.Value == null)
                return null;

            var streamValue = requestInfo.BodyParameterInfo.Value as Stream;
            if (streamValue != null)
                return new StreamContent(streamValue);

            var stringValue = requestInfo.BodyParameterInfo.Value as string;
            if (stringValue != null)
                return new StringContent(stringValue);

            switch (requestInfo.BodyParameterInfo.SerializationMethod)
            {
                case BodySerializationMethod.UrlEncoded:
                    return new FormUrlEncodedContent(new FormValueDictionary(requestInfo.BodyParameterInfo.Value));
                case BodySerializationMethod.Serialized:
                    return new StringContent(this.RequestBodySerializer.SerializeBody(requestInfo.BodyParameterInfo.Value));
                default:
                    throw new InvalidOperationException("Should never get here");
            }
        }

        protected virtual async Task<HttpResponseMessage> SendRequestAsync(RequestInfo requestInfo)
        {
            var message = new HttpRequestMessage()
            {
                Method = requestInfo.Method,
                RequestUri = this.ConstructUri(requestInfo),
                Content = this.ConstructContent(requestInfo),
            };

            // We're always going to want the content - we're a REST requesting library, and if there's a response we're always
            // going to parse it out before returning. If we use HttpCompletionOptions.ResponseContentRead, then our
            // CancellationToken will abort either the initial fetch *or* the read phases, which is what we want.
            var response = await this.httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead, requestInfo.CancellationToken).ConfigureAwait(false);

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

        public virtual async Task<Response<T>> RequestWithResponseAsync<T>(RequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            T deserializedResponse = await this.ResponseDeserializer.ReadAndDeserialize<T>(response, requestInfo.CancellationToken).ConfigureAwait(false);
            return new Response<T>(response, deserializedResponse);
        }
    }

    internal class FormValueDictionary : Dictionary<string, string>
    {
        public FormValueDictionary(object source)
        {
            if (source == null)
                return;

            var dictionary = source as IDictionary;
            if (dictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    this.Add(key.ToString(), (dictionary[key] ?? String.Empty).ToString());
                }
            }
        }
    }
}
