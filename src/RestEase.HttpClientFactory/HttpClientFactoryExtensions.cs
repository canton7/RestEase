using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RestEase.Implementation;

namespace RestEase.HttpClientFactory
{
    /// <summary>
    /// RestEase extension methods on <see cref="IServiceCollection"/>
    /// </summary>
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="basePath">Base path to use for requests (may be <c>null</c> if your interface only uses absolute paths, or has an absolute <c>[BasePath]</c>)</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            string? basePath,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services.AddRestEaseClientCore(typeof(T), ToUri(basePath), requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For<T>();
            });
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseUri">Base URI to use for requests (may be <c>null</c> if your interface only uses absolute paths, or has an absolute <c>[BasePath]</c>)</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Uri? baseUri,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services.AddRestEaseClientCore(typeof(T), baseUri, requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For<T>();
            });
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="services">Contains to add to</param>
        /// <param name="basePath">Base path to use for requests (may be <c>null</c> if your interface only uses absolute paths, or has an absolute <c>[BasePath]</c>)</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            string? basePath,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services.AddRestEaseClientCore(restEaseType, ToUri(basePath), requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For(restEaseType);
            });
        }

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="services">Contains to add to</param>
        /// <param name="baseUri">Base URI to use for requests (may be <c>null</c> if your interface only uses absolute paths, or has an absolute <c>[BasePath]</c>)</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseUri,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services.AddRestEaseClientCore(restEaseType, baseUri, requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For(restEaseType);
            });
        }

        private static IHttpClientBuilder AddRestEaseClientCore(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseUri,
            RequestModifier? requestModifier,
            Func<HttpClient, object> factory)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var builder = services.AddHttpClient(UniqueNameForType(restEaseType), httpClient =>
            {
                if (baseUri != null)
                {
                    httpClient.BaseAddress = baseUri;
                }
            }).AddTypedClient(factory);

            if (requestModifier != null)
            {
                builder = builder.ConfigurePrimaryHttpMessageHandler(() => new ModifyingClientHttpHandler(requestModifier));
            }

            return builder;
        }

        private static Uri? ToUri(string? uri) =>
            uri == null ? null : new Uri(uri);

        private static string UniqueNameForType(Type type)
        {
            var sb = new StringBuilder();
            Impl(type);
            return sb.ToString();

            void Impl(Type typeInfo)
            {
                if (typeInfo.IsGenericType)
                {
                    string fullName = type.GetGenericTypeDefinition().FullName;
                    sb.Append(fullName.Substring(0, fullName.LastIndexOf('`')));
                    sb.Append("<");
                    int i = 0;
                    foreach (var arg in typeInfo.GetGenericArguments())
                    {
                        if (i > 0)
                        {
                            sb.Append(",");
                        }
                        Impl(arg);
                        i++;
                    }
                    sb.Append(">");
                }
                else
                {
                    sb.Append(typeInfo.FullName);
                }
            }
        }
    }
}
