using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RestEase.HttpClientFactory
{
    public static partial class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Register the given RestEase interface type, without a Base Address. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services.AddRestEaseClient<T>((string?)null, configurer, requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface specifies
        /// <see cref="BaseAddressAttribute"/>, has an absolute <see cref="BasePathAttribute"/>, or only uses absolute
        /// paths)
        /// </param>
        /// <param name="clientConfigurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            string? baseAddress,
            Action<RestClient>? clientConfigurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services
                .CreateHttpClientBuilder(typeof(T), ToUri(baseAddress))
                .AddRestEaseClientCore(typeof(T), GenericFactory<T>(clientConfigurer, null), requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface specifies
        /// <see cref="BaseAddressAttribute"/>, has an absolute <see cref="BasePathAttribute"/>, or only uses absolute
        /// paths)
        /// </param>
        /// <param name="clientConfigurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Uri? baseAddress,
            Action<RestClient>? clientConfigurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services
                .CreateHttpClientBuilder(typeof(T), baseAddress)
                .AddRestEaseClientCore(typeof(T), GenericFactory<T>(clientConfigurer, null), requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="services">Contains to add to</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            return services.AddRestEaseClient(restEaseType, (string?)null, configurer, requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface has an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or only uses absolute paths)
        /// </param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            string? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services
                .CreateHttpClientBuilder(restEaseType, ToUri(baseAddress))
                .AddRestEaseClientCore(restEaseType, NonGenericFactory(restEaseType, configurer), requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface has an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or only uses absolute paths)
        /// </param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services
                .CreateHttpClientBuilder(restEaseType, baseAddress)
                .AddRestEaseClientCore(restEaseType, NonGenericFactory(restEaseType, configurer), requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="configurer">Delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder UseWithRestEaseClient<T>(
            this IHttpClientBuilder httpClientBuilder,
            Action<RestClient> configurer)
            where T : class
        {
            return httpClientBuilder.UseWithRestEaseClient<T>(configurer, null);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="clientConfigurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder UseWithRestEaseClient<T>(
            this IHttpClientBuilder httpClientBuilder,
            Action<RestClient>? clientConfigurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return httpClientBuilder.AddRestEaseClientCore(typeof(T), GenericFactory<T>(clientConfigurer, null), requestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="configurer">Delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder UseWithRestEaseClient(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType,
            Action<RestClient> configurer)
        {
            return httpClientBuilder.UseWithRestEaseClient(restEaseType, configurer, null);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IHttpClientBuilder UseWithRestEaseClient(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            return httpClientBuilder.AddRestEaseClientCore(restEaseType, NonGenericFactory(restEaseType, configurer), requestModifier);
        }
    }
}
