﻿using System;
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
        public ResponseDeserializer ResponseDeserializer { get; set; } = new JsonResponseDeserializer();

        /// <summary>
        /// Gets or sets the serializer used to serialize request bodies (when [Body(BodySerializationMethod.Serialized)] is used)
        /// </summary>
        public RequestBodySerializer RequestBodySerializer { get; set; } = new JsonRequestBodySerializer();

        /// <summary>
        /// Gets or sets the serializer used to serialize query parameters (when [Query(QuerySerializationMethod.Serialized)] is used)
        /// </summary>
        public RequestQueryParamSerializer RequestQueryParamSerializer { get; set; } = new JsonRequestQueryParamSerializer();

        /// <summary>
        /// Gets or sets the builder used to construct query strings, if any
        /// </summary>
        public QueryStringBuilder QueryStringBuilder { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFormatProvider"/> used to format items using <see cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// </summary>
        /// <remarks>
        /// Defaults to null, in which case the current culture is used.
        /// </remarks>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Requester"/> class, using the given HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient to use to make requests</param>
        public Requester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
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
                var serialized = pathParam.SerializeToString(this.FormatProvider);

                // Space needs to be treated separately
                var value = pathParam.UrlEncode ? WebUtility.UrlEncode(serialized.Value ?? String.Empty).Replace("+", "%20") : serialized.Value;
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

            string rawQueryParameter = requestInfo.RawQueryParameter?.SerializeToString(this.FormatProvider) ?? string.Empty;
            var query = this.BuildQueryParam(uriBuilder.Query, rawQueryParameter, requestInfo.QueryParams, requestInfo.QueryProperties, requestInfo);

            // Mono's UriBuilder.Query setter will always add a '?', so we can end up with a double '??'.
            uriBuilder.Query = query.TrimStart('?');

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Build up a query string from the initial query string, raw query parameter, and any query params (which need to be combined)
        /// </summary>
        /// <param name="initialQueryString">Initial query string, present from the URI the user specified in the Get/etc parameter</param>
        /// <param name="rawQueryParameter">The raw query parameter, if any</param>
        /// <param name="queryParams">The query parameters which need serializing (or an empty collection)</param>
        /// <param name="queryProperties">The query parameters from properties which need serialializing (or an empty collection)</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <returns>Query params combined into a query string</returns>
        protected virtual string BuildQueryParam(
            string initialQueryString,
            string rawQueryParameter,
            IEnumerable<QueryParameterInfo> queryParams,
            IEnumerable<QueryParameterInfo> queryProperties,
            IRequestInfo requestInfo)
        {
            var serializedQueryParams = queryParams.SelectMany(x => this.SerializeQueryParameter(x, requestInfo));
            var serializedQueryProperties = queryProperties.SelectMany(x => this.SerializeQueryParameter(x, requestInfo));

            if (this.QueryStringBuilder != null)
            {
                var info = new QueryStringBuilderInfo(initialQueryString, rawQueryParameter, serializedQueryParams, serializedQueryProperties, requestInfo, this.FormatProvider);
                return this.QueryStringBuilder.Build(info);
            }

            // Implementation copied from FormUrlEncodedContent

            var sb = new StringBuilder();

            void AppendQueryString(string query)
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append(query);
            }

            string Encode(string data)
            {
                if (string.IsNullOrEmpty(data))
                    return string.Empty;
                return Uri.EscapeDataString(data).Replace("%20", "+");
            }

            if (!String.IsNullOrEmpty(initialQueryString))
                AppendQueryString(initialQueryString.Replace("%20", "+"));
            if (!String.IsNullOrEmpty(rawQueryParameter))
                AppendQueryString(rawQueryParameter);

            foreach (var kvp in serializedQueryParams.Concat(serializedQueryProperties))
            {
                if (kvp.Key == null)
                {
                    AppendQueryString(Encode(kvp.Value));
                }
                else
                {
                    AppendQueryString(Encode(this.ToStringHelper(kvp.Key)));
                    sb.Append('=');
                    sb.Append(Encode(kvp.Value));
                }
            }

            return sb.ToString();
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
                        var stringValue = this.ToStringHelper(individualValue);
                        yield return new KeyValuePair<string, string>(this.ToStringHelper(kvp.Key), stringValue);
                    }
                }
                else if (kvp.Value != null)
                {
                    yield return new KeyValuePair<string, string>(this.ToStringHelper(kvp.Key), this.ToStringHelper(kvp.Value));
                }
            }
        }

        /// <summary>
        /// Serializes the value of a query parameter, using an appropriate method
        /// </summary>
        /// <param name="queryParameter">Query parameter to serialize</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <returns>Serialized value</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> SerializeQueryParameter(QueryParameterInfo queryParameter, IRequestInfo requestInfo)
        {
            switch (queryParameter.SerializationMethod)
            {
                case QuerySerializationMethod.ToString:
                    return queryParameter.SerializeToString(this.FormatProvider);
                case QuerySerializationMethod.Serialized:
                    if (this.RequestQueryParamSerializer == null)
                        throw new InvalidOperationException("Cannot serialize query parameter when RequestQueryParamSerializer is null. Please set RequestQueryParamSerializer");
                    var result = queryParameter.SerializeValue(this.RequestQueryParamSerializer, requestInfo);
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
            if (requestInfo.BodyParameterInfo == null)
            {
                return null;
            }

            if (typeof(HttpContent).GetTypeInfo().IsAssignableFrom(requestInfo.BodyParameterInfo.ObjectType.GetTypeInfo()))
            {
                return requestInfo.BodyParameterInfo.ObjectValue == null ? null : requestInfo.BodyParameterInfo.ObjectValue as HttpContent;
            }

            if (typeof(Stream).GetTypeInfo().IsAssignableFrom(requestInfo.BodyParameterInfo.ObjectType.GetTypeInfo()))
            {
                return requestInfo.BodyParameterInfo.ObjectValue == null ? null : new StreamContent(requestInfo.BodyParameterInfo.ObjectValue as Stream);
            }

            if (typeof(string).GetTypeInfo().IsAssignableFrom(requestInfo.BodyParameterInfo.ObjectType.GetTypeInfo()))
            {
                return requestInfo.BodyParameterInfo.ObjectValue == null ? null : new StringContent(requestInfo.BodyParameterInfo.ObjectValue as string);
            }

            if (typeof(byte[]).GetTypeInfo().IsAssignableFrom(requestInfo.BodyParameterInfo.ObjectType.GetTypeInfo()))
            {
                return requestInfo.BodyParameterInfo.ObjectValue == null ? null : new ByteArrayContent(requestInfo.BodyParameterInfo.ObjectValue as byte[]);
            }

            switch (requestInfo.BodyParameterInfo.SerializationMethod)
            {
                case BodySerializationMethod.UrlEncoded:
                    return new FormUrlEncodedContent(this.SerializeBodyForUrlEncoding(requestInfo.BodyParameterInfo.ObjectValue));
                case BodySerializationMethod.Serialized:
                    if (this.RequestBodySerializer == null)
                        throw new InvalidOperationException("Cannot serialize request body when RequestBodySerializer is null. Please set RequestBodySerializer");
                    return requestInfo.BodyParameterInfo.SerializeValue(this.RequestBodySerializer, requestInfo);
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
            this.ApplyHeadersSet(requestMessage, classHeaders.Concat(requestInfo.PropertyHeaders), false);
            this.ApplyHeadersSet(requestMessage, requestInfo.MethodHeaders.Concat(requestInfo.HeaderParams), true);
        }

        /// <summary>
        /// Given a set of headers, apply them to the given HttpRequestMessage. Headers will override any of that type already present
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to add the headers to</param>
        /// <param name="headers">Headers to add</param>
        /// <param name="complainIfBodyHeadersButNoBody">If true, and the header doesn't apply to the request, and there's no body, throw. If false, and the header can apply to a body but there isn't one, don't throw</param>
        protected virtual void ApplyHeadersSet(HttpRequestMessage requestMessage, IEnumerable<KeyValuePair<string, string>> headers, bool complainIfBodyHeadersButNoBody)
        {
            HttpContent dummyContent = null;
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
                if (!added)
                {
                    if (requestMessage.Content != null)
                    {
                        if (requestMessage.Content.Headers.Any(x => x.Key == headersGroup.Key))
                            requestMessage.Content.Headers.Remove(headersGroup.Key);
                        added = requestMessage.Content.Headers.TryAddWithoutValidation(headersGroup.Key, headersToAdd);
                    }
                    else if (!complainIfBodyHeadersButNoBody)
                    {
                        // See if it could be added to a content
                        if (dummyContent == null)
                            dummyContent = new ByteArrayContent(new byte[0]);
                        added = dummyContent.Headers.TryAddWithoutValidation(headersGroup.Key, headersToAdd);
                    }
                }

                if (!added)
                    throw new ArgumentException(String.Format("Header {0} could not be added. Maybe it's a content-related header but there's no content, or it's associated with HTTP responses, or it's malformed?", headersGroup.Key));
            }
        }

        /// <summary>
        /// Serializes an item to a string using <see cref="FormatProvider"/> if the object implements <see cref="IFormattable"/>
        /// </summary>
        /// <typeparam name="T">Type of the value being serialized</typeparam>
        /// <param name="value">Value being serialized</param>
        /// <returns>Serialized value</returns>
        protected string ToStringHelper<T>(T value) => Implementation.ToStringHelper.ToString(value, null, this.FormatProvider);

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
                throw await ApiException.CreateAsync(message, response).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// Calls this.ResponseDeserializer.ReadAndDeserializeAsync, after checking it's not null
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize into</typeparam>
        /// <param name="content">String content read from the response</param>
        /// <param name="response">Response to deserialize from</param>
        /// <param name="requestInfo">RequestInfo representing the request</param>
        /// <returns>A task containing the deserialized response</returns>
        protected virtual T Deserialize<T>(string content, HttpResponseMessage response, IRequestInfo requestInfo)
        {
            if (this.ResponseDeserializer == null)
                throw new InvalidOperationException("Cannot deserialize a response when ResponseDeserializer is null. Please set ResponseDeserializer");
            return this.ResponseDeserializer.Deserialize<T>(content, response, new ResponseDeserializerInfo(requestInfo));
        }

        /// <summary>
        /// Called from interface methods which return a Task
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task which completes when the request completed</returns>
        public virtual async Task RequestVoidAsync(IRequestInfo requestInfo)
        {
            // We're not going to return the body (unless there's an ApiException), so there's no point in fetching it
            using (await this.SendRequestAsync(requestInfo, readBody: false).ConfigureAwait(false))
            {
            }
        }

        /// <summary>
        /// Called from interface methods which return a Task{CustomType}. Deserializes and returns the response
        /// </summary>
        /// <typeparam name="T">Type of the response, to deserialize into</typeparam>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task resulting in the deserialized response</returns>
        public virtual async Task<T> RequestAsync<T>(IRequestInfo requestInfo)
        {
            using (var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false))
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                T deserializedResponse = this.Deserialize<T>(content, response, requestInfo);
                return deserializedResponse;
            }
        }

        /// <summary>
        /// Called from interface methods which return a Task{HttpResponseMessage}
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task containing the result of the request</returns>
        public virtual async Task<HttpResponseMessage> RequestWithResponseMessageAsync(IRequestInfo requestInfo)
        {
            // It's the user's responsibility to dispose this
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
            // It's the user's responsibility to dispose the Response<T>, which disposes the HttpResponseMessage
            var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new Response<T>(content, response, () => this.Deserialize<T>(content, response, requestInfo));
        }

        /// <summary>
        /// Called from interface methods which return a Task{string}
        /// </summary>
        /// <param name="requestInfo">IRequestInfo to construct the request from</param>
        /// <returns>Task containing the raw string body of the response</returns>
        public virtual async Task<string> RequestRawAsync(IRequestInfo requestInfo)
        {
            using (var response = await this.SendRequestAsync(requestInfo, readBody: true).ConfigureAwait(false))
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseString;
            }
        }

        /// <summary>
        /// Invoked when the API interface method being called returns a Task{Stream}
        /// </summary>
        /// <param name="requestInfo">Object holding all information about the request</param>
        /// <returns>Task to return to the API interface caller</returns>
        public virtual async Task<Stream> RequestStreamAsync(IRequestInfo requestInfo)
        {
            // Disposing the HttpResponseMessage will dispose the Stream (indeed, that's the only reason when
            // HttpResponseMessage is IDisposable), which the user wants to use. Since the HttpResponseMessage
            // is only IDisposable to dispose the Stream, provided that the user disposes the Stream themselves,
            // nothing will leak.
            var response = await this.SendRequestAsync(requestInfo, readBody: false).ConfigureAwait(false);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
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
