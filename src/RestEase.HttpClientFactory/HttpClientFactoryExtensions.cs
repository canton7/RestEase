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
        /// Register the given RestEase interface type, without a Base Address. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
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
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            string? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services.AddRestEaseClientCore(typeof(T), ToUri(baseAddress), requestModifier, httpClient =>
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
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface specifies
        /// <see cref="BaseAddressAttribute"/>, has an absolute <see cref="BasePathAttribute"/>, or only uses absolute
        /// paths)
        /// </param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Uri? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return services.AddRestEaseClientCore(typeof(T), baseAddress, requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For<T>();
            });
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
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            string? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services.AddRestEaseClientCore(restEaseType, ToUri(baseAddress), requestModifier, httpClient =>
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
        /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface has an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or only uses absolute paths)
        /// </param>
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));

            return services.AddRestEaseClientCore(restEaseType, baseAddress, requestModifier, httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For(restEaseType);
            });
        }

        private static IHttpClientBuilder AddRestEaseClientCore(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress,
            RequestModifier? requestModifier,
            Func<HttpClient, object> factory)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var builder = services.AddHttpClient(UniqueNameForType(restEaseType), httpClient =>
            {
                if (baseAddress != null)
                {
                    httpClient.BaseAddress = baseAddress;
                }
            });

            // See https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientBuilderExtensions.cs
            builder.Services.AddTransient(restEaseType, serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(builder.Name);
                return factory(httpClient);
            });

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
