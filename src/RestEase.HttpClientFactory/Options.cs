using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RestEase.HttpClientFactory
{
    /// <summary>
    /// Additional options which can be passed to
    /// <see cref="HttpClientFactoryExtensions.AddRestEaseClient(IServiceCollection, Type, AddRestEaseClientOptions)"/>
    /// and similar overloads
    /// </summary>
    public class AddRestEaseClientOptions
    {
        /// <summary>
        /// Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)
        /// </summary>
        public Action<RestClient>? RestClientConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to use to modify all requests
        /// </summary>
        public RequestModifier? RequestModifier { get; set; }

        /// <summary>
        /// Optional factory to create an <see cref="IRequester"/> instance to use
        /// </summary>
        /// <remarks>
        /// If specified, <see cref="RestClientConfigurer"/> must be null, or an exception will be thrown
        /// </remarks>
        public Func<HttpClient, IRequester>? RequesterFactory { get; set; }
    }

    /// <summary>
    /// Additional options which can be passed to
    /// <see cref="HttpClientFactoryExtensions.AddRestEaseClient{T}(IServiceCollection, AddRestEaseClientOptions{T})"/>
    /// and similar overloads
    /// </summary>
    public class AddRestEaseClientOptions<T> where T : class
    {
        /// <summary>
        /// Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)
        /// </summary>
        public Action<RestClient>? RestClientConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to configure the <typeparamref name="T"/> (to set properties, etc)
        /// </summary>
        public Action<T>? InstanceConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to use to modify all requests
        /// </summary>
        public RequestModifier? RequestModifier { get; set; }

        /// <summary>
        /// Optional factory to create an <see cref="IRequester"/> instance to use
        /// </summary>
        /// <remarks>
        /// If specified, <see cref="RestClientConfigurer"/> must be null, or an exception will be thrown
        /// </remarks>
        public Func<HttpClient, IRequester>? RequesterFactory { get; set; }
    }

    /// <summary>
    /// Additional options which can be passed to
    /// <see cref="HttpClientFactoryExtensions.UseWithRestEaseClient(IHttpClientBuilder, Type, UseWithRestEaseClientOptions)"/>
    /// and similar overloads
    /// </summary>
    public class UseWithRestEaseClientOptions
    {
        /// <summary>
        /// Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)
        /// </summary>
        public Action<RestClient>? RestClientConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to use to modify all requests
        /// </summary>
        public RequestModifier? RequestModifier { get; set; }

        /// <summary>
        /// Optional factory to create an <see cref="IRequester"/> instance to use
        /// </summary>
        /// <remarks>
        /// If specified, <see cref="RestClientConfigurer"/> must be null, or an exception will be thrown
        /// </remarks>
        public Func<HttpClient, IRequester>? RequesterFactory { get; set; }
    }

    /// <summary>
    /// Additional options which can be passed to
    /// <see cref="HttpClientFactoryExtensions.UseWithRestEaseClient{T}(IHttpClientBuilder, UseWithRestEaseClientOptions{T})"/>
    /// and similar overloads
    /// </summary>
    public class UseWithRestEaseClientOptions<T> where T : class
    {
        /// <summary>
        /// Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)
        /// </summary>
        public Action<RestClient>? RestClientConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to configure the <typeparamref name="T"/> (to set properties, etc)
        /// </summary>
        public Action<T>? InstanceConfigurer { get; set; }

        /// <summary>
        /// Optional delegate to use to modify all requests
        /// </summary>
        public RequestModifier? RequestModifier { get; set; }

        /// <summary>
        /// Optional factory to create an <see cref="IRequester"/> instance to use
        /// </summary>
        /// <remarks>
        /// If specified, <see cref="RestClientConfigurer"/> must be null, or an exception will be thrown
        /// </remarks>
        public Func<HttpClient, IRequester>? RequesterFactory { get; set; }
    }
}
