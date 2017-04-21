using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestEase.Platform;
using System.Reflection;

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
        /// Gets or sets the serializer used to serialize request bodies (when [Body(BodySerializationMethod.Serialized)] is used)
        /// </summary>
        public IRequestBodySerializer RequestBodySerializer { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize query parameters (when [Query(QuerySerializationMethod.Serialized)] is used)
        /// </summary>
        public IRequestQueryParamSerializer RequestQueryParamSerializer { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Requester"/> class, using the given HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.ResponseDeserializer = new JsonResponseDeserializer();
            this.RequestBodySerializer = new JsonRequestBodySerializer();
            this.RequestQueryParamSerializer = new JsonRequestQueryParamSerializer();
        }

        /// <summary>
        /// Takes the Path, PathParams, and PathProperties from the given IRequestInfo, and constructs a path with placeholders substituted
        /// for their desired values.
        /// </summary>
        /// <remarks>
        /// Note that this method assumes that valdation has occurred. That is, there won't by any
        /// placeholders with no value, or values without a placeholder.
        /// </remarks>
        /// <param name="requestInfo">IRequestInfo to get Path, PathParams, and PathProperties from</param>
        /// <returns>The constructed path, with placeholders substituted for their actual values</returns>
        protected virtual string SubstitutePathParameters(IRequestInfo requestInfo)
        {
            if (requestInfo.Path == null || (!requestInfo.PathParams.Any() && !requestInfo.PathProperties.Any()))
                return requestInfo.Path;

            // We've already done validation to ensure that the parts in the path, and the available values, are present
            // Substitute the path params, then the path properties. That way, the properties are used only if
            // there are no matching path params.
            var sb = new StringBuilder(requestInfo.Path);
            foreach (var pathParam in requestInfo.PathParams.Concat(requestInfo.PathProperties))
            {
                var serialized = pathParam.SerializeToString();

                // Space needs to be treated separately
                var value = pathParam.UrlEncode ? UrlEncode(serialized.Value ?? String.Empty).Replace("+", "%20") : serialized.Value;
                sb.Replace("{" + (serialized.Key ?? String.Empty) + "}", value);
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
                var trimmedPath = (path ?? String.Empty).TrimStart('/');
                // Here, a leading slash will strip the path from baseAddress
                var uri = new Uri(trimmedPath, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    var baseAddress = this.httpClient.BaseAddress?.ToString();
                    if (!String.IsNullOrWhiteSpace(baseAddress))
                    {
                        // Need to make sure it ends with a trailing slash, or appending our relative path will strip
                        // the last path component (assuming we *have* a relative path)
                        if (!baseAddress.EndsWith("/") && !String.IsNullOrWhiteSpace(trimmedPath))
                            baseAddress += '/';
                        uri = new Uri(new Uri(baseAddress), uri);
                    }
                }

                // If it's still relative, 'new UriBuilder(Uri)' won't accept it, but 'new UriBuilder(string)' will
                // (by prepending 'http://').
                uriBuilder = uri.IsAbsoluteUri ? new UriBuilder(uri) : new UriBuilder(uri.ToString());
            }
            catch (FormatException e)
            {
                // The original exception doesn't actually include the path - which is not helpful to the user
                throw new FormatException(String.Format("Path '{0}' is not valid: {1}", path, e.Message));
            }

            string initialQueryString = uriBuilder.Query;
            if (requestInfo.RawQueryParameter != null)
            {
                var rawQueryParameter = requestInfo.RawQueryParameter.SerializeToString();
                if (String.IsNullOrEmpty(initialQueryString))
                    initialQueryString = rawQueryParameter;
                else
                    initialQueryString += "&" + rawQueryParameter;
            }

            var queryParams = requestInfo.QueryParams.SelectMany(x => this.SerializeQueryParameter(x));
            // Mono's UriBuilder.Query setter will always add a '?', so we can end up with a double '??'
            uriBuilder.Query = QueryParamBuilder.Build(initialQueryString, queryParams).TrimStart('?');

            return uriBuilder.Uri;
        }

        private string UrlEncode(string uri)
        {
            return WebUtility.UrlEncode(uri);
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
                throw new ArgumentException("BodySerializationMethod is UrlEncoded, but body does not implement IDictionary or IDictionary<TKey, TValue>", nameof(body));
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
                        var stringValue = individualValue?.ToString();
                        yield return new KeyValuePair<string, string>(kvp.Key.ToString(), stringValue);
                    }
                }
                else if (kvp.Value != null)
                {
                    yield return new KeyValuePair<string, string>(kvp.Key.ToString(), kvp.Value.ToString());
                }
            }
        }

        /// <summary>
        /// Serializes the value of a query parameter, using an appropriate method
        /// </summary>
        /// <param name="queryParameter">Query parameter to serialize</param>
        /// <returns>Serialized value</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> SerializeQueryParameter(QueryParameterInfo queryParameter)
        {
            switch (queryParameter.SerializationMethod)
            {
                case QuerySerializationMethod.ToString:
                    return queryParameter.SerializeToString();
                case QuerySerializationMethod.Serialized:
                    if (this.RequestQueryParamSerializer == null)
                        throw new InvalidOperationException("Cannot serialize query parameter when RequestQueryParamSerializer is null. Please set RequestQueryParamSerializer");
                    var result = queryParameter.SerializeValue(this.RequestQueryParamSerializer);
                    return result ?? Enumerable.Empty<KeyValuePair<string, string>>();
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

            if (requestInfo.BodyParameterInfo.ObjectValue is HttpContent httpContentValue)
                return httpContentValue;

            if (requestInfo.BodyParameterInfo.ObjectValue is Stream streamValue)
                return new StreamContent(streamValue);

            if (requestInfo.BodyParameterInfo.ObjectValue is string stringValue)
                return new StringContent(stringValue);

            switch (requestInfo.BodyParameterInfo.SerializationMethod)
            {
                case BodySerializationMethod.UrlEncoded:
                    return new FormUrlEncodedContent(this.SerializeBodyForUrlEncoding(requestInfo.BodyParameterInfo.ObjectValue));
                case BodySerializationMethod.Serialized:
                    if (this.RequestBodySerializer == null)
                        throw new InvalidOperationException("Cannot serialize request body when RequestBodySerializer is null. Please set RequestBodySerializer");
                    return requestInfo.BodyParameterInfo.SerializeValue(this.RequestBodySerializer);
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
        /// <param name="readBody">True to pass HttpCompletionOption.ResponseContentRead, meaning that the body is read here</param>
        /// <returns>Resulting HttpResponseMessage</returns>
        protected virtual async Task<HttpResponseMessage> SendRequestAsync(IRequestInfo requestInfo, bool readBody)
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

            // If we're deserializing, we're always going to want the content, since we're always going to deserialize it.
            // Therefore use HttpCompletionOption.ResponseContentRead so that the content gets read at this point, meaning
            // that it can be cancelled by our CancellationToken.
            // However if we're returning a HttpResponseMessage, that's probably because the user wants to read it themselves.
            // They might want to only fetch the first bit, or stream it into a file, or get process on it. In this case,
            // we'll want HttpCompletionOption.ResponseHeadersRead.

            var completionOption = readBody ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;
            var response = await this.httpClient.SendAsync(message, completionOption, requestInfo.CancellationToken).ConfigureAwait(false);

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
            // We're not going to return the body (unless there's an ApiException), so there's no point in fetching it
            await this.SendRequestAsync(requestInfo, readBody: false).ConfigureAwait(false);
        }

        /// <summary>
        /// Called from interface methods which return a Task{CustomType}. Deserializes and returns the response
        /// </summary>
        /// <typeparam name="T">Type of the response, to deserialize into</typeparam>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task resulting in the deserialized response</returns>
        public virtual async Task<T> RequestAsync<T>(IRequestInfo requestInfo)
        {
            var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false);
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
            var response = await this.SendRequestAsync(requestInfo, readBody: false).ConfigureAwait(false);
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
            var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false);
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
            var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString;
        }

        /// <summary>
        /// Disposes the underlying <see cref="HttpClient"/>
        /// </summary>
        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}
