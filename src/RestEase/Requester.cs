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

        protected virtual string SubstitutePathParameters(RequestInfo requestInfo)
        {
            if (requestInfo.Path == null || requestInfo.PathParams.Count == 0)
                return requestInfo.Path;

            // We've already done validation to ensure that the parts in the path, and the available values, are present
            var sb = new StringBuilder(requestInfo.Path);
            foreach (var pathParam in requestInfo.PathParams)
            {
                sb.Replace("{" + pathParam.Key + "}", pathParam.Value);
            }

            return sb.ToString();
        }

        protected virtual Uri ConstructUri(RequestInfo requestInfo)
        {
            // UriBuilder insists that we provide it with an absolute URI, even though we only want a relative one...
            var relativePath = this.SubstitutePathParameters(requestInfo) ?? String.Empty;
            var uriBuilder = new UriBuilder(new Uri(new Uri("http://api"), relativePath));

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

        protected virtual void ApplyHeaders(RequestInfo requestInfo, HttpRequestMessage requestMessage)
        {
            // Apply from class -> method -> params, so we get the proper hierarchy
            this.AppleHeadersSet(requestMessage, this.SplitHeaders(requestInfo.ClassHeaders));
            this.AppleHeadersSet(requestMessage, this.SplitHeaders(requestInfo.MethodHeaders));
            this.AppleHeadersSet(requestMessage, requestInfo.HeaderParams);
        }

        protected virtual IEnumerable<KeyValuePair<string, string>> SplitHeaders(List<string> headers)
        {
            var splitHeaders = from header in headers
                               where !String.IsNullOrWhiteSpace(header)
                               let parts = header.Split(new[] { '.' }, 1)
                               select new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1] : null);
            return splitHeaders;
        }

        protected virtual void AppleHeadersSet(HttpRequestMessage requestMessage, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var headersGroups = headers.GroupBy(x => x.Key);

            foreach (var headersGroup in headersGroups)
            {
                // Can't use .Contains, as it will throw if the header isn't a valid type
                if (requestMessage.Headers.Any(x => x.Key == headersGroup.Key))
                    requestMessage.Headers.Remove(headersGroup.Key);

                // Empty collection = "remove all instances of this header only"
                if (!headersGroup.Any())
                    continue;

                bool added = requestMessage.Headers.TryAddWithoutValidation(headersGroup.Key, headersGroup.Select(x => x.Value));
                
                // If we failed, it's probably a content header. Try again there
                if (!added && requestMessage.Content != null)
                {
                    if (requestMessage.Content.Headers.Any(x => x.Key == headersGroup.Key))
                        requestMessage.Content.Headers.Remove(headersGroup.Key);
                    added = requestMessage.Content.Headers.TryAddWithoutValidation(headersGroup.Key, headersGroup.Select(x => x.Value));
                }

                if (!added)
                    throw new ArgumentException(String.Format("Header {0} could not be added. Maybe it's a content-related header but there's no content?", headersGroup.Key));
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

            // Do this after setting the content, as doing so may set headers which we want to remove / override
            this.ApplyHeaders(requestInfo, message);

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
