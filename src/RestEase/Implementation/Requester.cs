using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RestEase.Implementation
{
    /// <summary>
    /// Clas used by generated implementations to make HTTP requests
    /// </summary>
    public class Requester : IRequester
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Gets or sets the deserializer used to deserialize responses
        /// </summary>
        public IResponseDeserializer ResponseDeserializer { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize request bodies (when [Body(BodySerializationMethod.Serialized)] is used) or query parameters (when [Query(QuerySerializationMethod.Serialized)] is used)
        /// </summary>
        public IRequestSerializer RequestSerializer { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Requester"/> class, using the given HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.ResponseDeserializer = new JsonResponseDeserializer();
            this.RequestSerializer = new JsonRequestSerializer();
        }

        /// <summary>
        /// Takes the Path and PathParams from the given IRequestInfo, and constructs a path with placeholders substituted
        /// for their desired values.
        /// </summary>
        /// <remarks>
        /// Note that this method assumes that valdation has occurred. That is, there won't by any
        /// placeholders with no value, or values without a placeholder.
        /// </remarks>
        /// <param name="requestInfo">IRequestInfo to get Path and PathParams from</param>
        /// <returns>The constructed path, with placeholders substituted for their actual values</returns>
        protected virtual string SubstitutePathParameters(IRequestInfo requestInfo)
        {
            if (requestInfo.Path == null || requestInfo.PathParams.Count == 0)
                return requestInfo.Path;

            // We've already done validation to ensure that the parts in the path, and the available values, are present
            var sb = new StringBuilder(requestInfo.Path);
            foreach (var pathParam in requestInfo.PathParams)
            {
                sb.Replace("{" + (pathParam.Key ?? String.Empty) + "}", pathParam.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Given an IRequestInfo and pre-substituted relative path, constructs a URI with the right query parameters
        /// </summary>
        /// <param name="path">Path to start with, with placeholders already substituted</param>
        /// <param name="requestInfo">IRequestInfo to retrieve the query parameters from</param>
        /// <returns>Constructed URI; relative if 'path' was relative, otherwise absolute</returns>
        protected virtual Uri ConstructUri(string path, IRequestInfo requestInfo)
        {
            UriBuilder uriBuilder;
            try
            {
                var trimmedPath = path.TrimStart('/');
                // Here, a leading slash will strip the path from baseAddress
                var uri = new Uri(trimmedPath, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    var baseAddress = this.httpClient.BaseAddress.ToString();
                    // Need to make sure it ends with a trailing slash, or appending our relative path will strip
                    // the last path component (assuming we *have* a relative path)
                    if (!baseAddress.EndsWith("/") && !String.IsNullOrWhiteSpace(trimmedPath))
                        baseAddress += '/';
                    uri = new Uri(new Uri(baseAddress), uri);
                }
                uriBuilder = new UriBuilder(uri);
            }
            catch (UriFormatException e)
            {
                // The original exception doesn't actually include the path - which is not helpful to the user
                throw new UriFormatException(String.Format("Path {0} is not valid: {1}", path, e.Message));
            }

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var queryParam in requestInfo.QueryParams)
            {
                var serialized = this.SerializeQueryParameterValue(queryParam);
                if (serialized != null)
                    query.Add(queryParam.Name, serialized);
            }
            if (requestInfo.QueryMap != null)
            { 
                // ImplementationBuilder asserts that we can iterate the QueryMap type
                foreach (var queryParam in this.TransformDictionaryToCollectionOfKeysAndValues(requestInfo.QueryMap))
                {
                    if (queryParam.Value != null)
                        query.Add(queryParam.Key, queryParam.Value);
                }
            }
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Given an object, attempt to serialize it into a form suitable for URL Encoding
        /// </summary>
        /// <remarks>Currently only supports objects which implement IDictionary</remarks>
        /// <param name="body">Object to attempt to serialize</param>
        /// <returns>Key/value collection suitable for URL encoding</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> SerializeBodyForUrlEncoding(object body)
        {
            if (body == null)
                return Enumerable.Empty<KeyValuePair<string, string>>();

            if (DictionaryIterator.CanIterate(body.GetType()))
                return this.TransformDictionaryToCollectionOfKeysAndValues(body);
            else
                throw new ArgumentException("BodySerializationMethod is UrlEncoded, but body does not implement IDictionary or IDictionary<TKey, TValue>");
        }

        /// <summary>
        /// Takes an IDictionary or IDictionary{TKey, TValue}, and emits KeyValuePairs for each key
        /// Takes account of IEnumerable values, null values, etc
        /// </summary>
        /// <param name="dictionary">Dictionary to transform</param>
        /// <returns>A set of KeyValuePairs</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> TransformDictionaryToCollectionOfKeysAndValues(object dictionary)
        {
            foreach (var kvp in DictionaryIterator.Iterate(dictionary))
            {
                if (kvp.Value != null && !(kvp.Value is string) && kvp.Value is IEnumerable)
                {
                    foreach (var individualValue in (IEnumerable)kvp.Value)
                    {
                        var stringValue = individualValue == null ? null : individualValue.ToString();
                        yield return new KeyValuePair<string, string>(kvp.Key.ToString(), stringValue);
                    }
                }
                else if (kvp.Value != null)
                {
                    yield return new KeyValuePair<string, string>(kvp.Key.ToString(), kvp.Value.ToString());
                }
            }
        }

        protected virtual string SerializeQueryParameterValue(QueryParameterInfo queryParameter)
        {
            if (queryParameter.ObjectValue == null)
                return null;

            switch (queryParameter.SerializationMethod)
            {
                case QuerySerialializationMethod.ToString:
                    return queryParameter.ObjectValue.ToString();
                case QuerySerialializationMethod.Serialized:
                    if (this.RequestSerializer == null)
                        throw new InvalidOperationException("Cannot serialize query parameter when RequestBodySerializer is null. Please set RequestSerializer");
                    return queryParameter.SerializeValue(this.RequestSerializer);
                default:
                    throw new InvalidOperationException("Should never get here");
            }
        }

        /// <summary>
        /// Given an IRequestInfo which may have a BodyParameterInfo, construct a suitable HttpContent for it if possible
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to get the BodyParameterInfo for</param>
        /// <returns>null if no body is set, otherwise a suitable HttpContent (StringContent, StreamContent, FormUrlEncodedContent, etc)</returns>
        protected virtual HttpContent ConstructContent(IRequestInfo requestInfo)
        {
            if (requestInfo.BodyParameterInfo == null || requestInfo.BodyParameterInfo.ObjectValue == null)
                return null;

            var httpContentValue = requestInfo.BodyParameterInfo.ObjectValue as HttpContent;
            if (httpContentValue != null)
                return httpContentValue;

            var streamValue = requestInfo.BodyParameterInfo.ObjectValue as Stream;
            if (streamValue != null)
                return new StreamContent(streamValue);

            var stringValue = requestInfo.BodyParameterInfo.ObjectValue as string;
            if (stringValue != null)
                return new StringContent(stringValue);

            switch (requestInfo.BodyParameterInfo.SerializationMethod)
            {
                case BodySerializationMethod.UrlEncoded:
                    return new FormUrlEncodedContent(this.SerializeBodyForUrlEncoding(requestInfo.BodyParameterInfo.ObjectValue));
                case BodySerializationMethod.Serialized:
                    if (this.RequestSerializer == null)
                        throw new InvalidOperationException("Cannot serialize request body when RequestSerializer is null. Please set RequestSerializer");
                    return requestInfo.BodyParameterInfo.SerializeValue(this.RequestSerializer);
                default:
                    throw new InvalidOperationException("Should never get here");
            }
        }

        /// <summary>
        /// Given an IRequestInfo containing a number of class/method/param headers, and a HttpRequestMessage,
        /// add the headers to the message, taing priority and overriding into account
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to get the headers from</param>
        /// <param name="requestMessage">HttpRequestMessage to add the headers to</param>
        protected virtual void ApplyHeaders(IRequestInfo requestInfo, HttpRequestMessage requestMessage)
        {
            // Apply from class -> method (combining static/dynamic), so we get the proper hierarchy
            var classHeaders = requestInfo.ClassHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>();
            this.ApplyHeadersSet(requestMessage, classHeaders.Concat(requestInfo.PropertyHeaders));
            this.ApplyHeadersSet(requestMessage, requestInfo.MethodHeaders.Concat(requestInfo.HeaderParams));
        }

        /// <summary>
        /// Given a set of headers, apply them to the given HttpRequestMessage. Headers will override any of that type already present
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to add the headers to</param>
        /// <param name="headers">Headers to add</param>
        protected virtual void ApplyHeadersSet(HttpRequestMessage requestMessage, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var headersGroups = headers.GroupBy(x => x.Key);

            foreach (var headersGroup in headersGroups)
            {
                // Can't use .Contains, as it will throw if the header isn't a valid type
                if (requestMessage.Headers.Any(x => x.Key == headersGroup.Key))
                    requestMessage.Headers.Remove(headersGroup.Key);

                // Null values are used to remove instances of a header, but should not be added
                var headersToAdd = headersGroup.Select(x => x.Value).Where(x => x != null).ToArray();
                if (!headersToAdd.Any())
                    continue;

                bool added = requestMessage.Headers.TryAddWithoutValidation(headersGroup.Key, headersToAdd);
                
                // If we failed, it's probably a content header. Try again there
                if (!added && requestMessage.Content != null)
                {
                    if (requestMessage.Content.Headers.Any(x => x.Key == headersGroup.Key))
                        requestMessage.Content.Headers.Remove(headersGroup.Key);
                    added = requestMessage.Content.Headers.TryAddWithoutValidation(headersGroup.Key, headersToAdd);
                }

                if (!added)
                    throw new ArgumentException(String.Format("Header {0} could not be added. Maybe it's a content-related header but there's no content?", headersGroup.Key));
            }
        }

        /// <summary>
        /// Given an IRequestInfo, construct a HttpRequestMessage, send it, check the response for success, then return it
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Resulting HttpResponseMessage</returns>
        protected virtual async Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo)
        {
            var path = this.SubstitutePathParameters(requestInfo) ?? String.Empty;
            var message = new HttpRequestMessage()
            {
                Method = requestInfo.Method,
                RequestUri = this.ConstructUri(path, requestInfo),
                Content = this.ConstructContent(requestInfo),
            };

            // Do this after setting the content, as doing so may set headers which we want to remove / override
            this.ApplyHeaders(requestInfo, message);

            // We're always going to want the content - we're a REST requesting library, and if there's a response we're always
            // going to parse it out before returning. If we use HttpCompletionOptions.ResponseContentRead, then our
            // CancellationToken will abort either the initial fetch *or* the read phases, which is what we want.
            var response = await this.httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead, requestInfo.CancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && !requestInfo.AllowAnyStatusCode)
                throw await ApiException.CreateAsync(response).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// Calls this.ResponseDeserializer.ReadAndDeserializeAsync, after checking it's not null
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">Response to deserialize from</param>
        /// <returns>A task containing the deserialized response</returns>
        protected virtual T Deserialize<T>(string content, HttpResponseMessage response)
        {
            if (this.ResponseDeserializer == null)
                throw new InvalidOperationException("Cannot deserialize a response when ResponseDeserializer is null. Please set ResponseDeserializer");
            return this.ResponseDeserializer.Deserialize<T>(content, response);
        }

        /// <summary>
        /// Called from interface methods which return a Task
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task which completes when the request completed</returns>
        public virtual async Task RequestVoidAsync(IRequestInfo requestInfo)
        {
            await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Called from interface methods which return a Task{CustomType}. Deserializes and returns the response
        /// </summary>
        /// <typeparam name="T">Type of the response, to deserialize into</typeparam>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task resulting in the deserialized response</returns>
        public virtual async Task<T> RequestAsync<T>(IRequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            T deserializedResponse = this.Deserialize<T>(content, response);
            return deserializedResponse;
        }

        /// <summary>
        /// Called from interface methods which return a Task{HttpResponseMessage}
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task containing the result of the request</returns>
        public virtual async Task<HttpResponseMessage> RequestWithResponseMessageAsync(IRequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Called from interface methods which return a Task{Response{T}}
        /// </summary>
        /// <typeparam name="T">Type of the response, to deserialize into</typeparam>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task containing a Response{T}, which contains the raw HttpResponseMessage, and its deserialized content</returns>
        public virtual async Task<Response<T>> RequestWithResponseAsync<T>(IRequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new Response<T>(content, response, () => this.Deserialize<T>(content, response));
        }

        /// <summary>
        /// Called from interface methods which return a Task{string}
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task containing the raw string body of the response</returns>
        public virtual async Task<string> RequestRawAsync(IRequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString;
        }
    }
}
