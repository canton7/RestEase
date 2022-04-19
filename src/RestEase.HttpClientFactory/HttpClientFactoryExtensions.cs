using System;
using System.ComponentModel;
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
            return services
                .CreateHttpClientBuilder(typeof(T), ToUri(baseAddress))
                .AddRestEaseClientCore(typeof(T), GenericFactory<T>(configurer), requestModifier);
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
            return services
                .CreateHttpClientBuilder(typeof(T), baseAddress)
                .AddRestEaseClientCore(typeof(T), GenericFactory<T>(configurer), requestModifier);
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
        /// <param name="configurer">Optional delegate to configure the <see cref="RestClient"/> (to set serializers, etc)</param>
        /// <param name="requestModifier">Optional delegate to use to modify all requests</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder UseWithRestEaseClient<T>(
            this IHttpClientBuilder httpClientBuilder,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
            where T : class
        {
            return httpClientBuilder.AddRestEaseClientCore(typeof(T), GenericFactory<T>(configurer), requestModifier);
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
        public static IHttpClientBuilder UseWithRestEaseClient(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType,
            Action<RestClient>? configurer = null,
            RequestModifier? requestModifier = null)
        {
            return httpClientBuilder.AddRestEaseClientCore(restEaseType, NonGenericFactory(restEaseType, configurer), requestModifier);
        }

        private static IHttpClientBuilder CreateHttpClientBuilder(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var builder = baseAddress == null
                ? services.AddHttpClient(UniqueNameForType(restEaseType))
                : services.AddHttpClient(UniqueNameForType(restEaseType), httpClient => httpClient.BaseAddress = baseAddress);

            return builder;
        }

        private static IHttpClientBuilder AddRestEaseClientCore(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType,
            Func<HttpClient, object> factory,
            RequestModifier? requestModifier)
        {
            if (httpClientBuilder is null)
                throw new ArgumentNullException(nameof(httpClientBuilder));

            // See https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientBuilderExtensions.cs
            httpClientBuilder.Services.AddTransient(restEaseType, serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(httpClientBuilder.Name);
                return factory(httpClient);
            });

            if (requestModifier != null)
            {
                httpClientBuilder.AddHttpMessageHandler(() => new ModifyingClientHttpHandler(requestModifier));
            }

            return httpClientBuilder;
        }

        private static Uri? ToUri(string? uri) =>
            uri == null ? null : new Uri(uri);

        private static Func<HttpClient, object> GenericFactory<T>(Action<RestClient>? configurer) where T : class
        {
            return httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For<T>();
            };
        }

        private static Func<HttpClient, object> NonGenericFactory(Type restEaseType, Action<RestClient>? configurer)
        {
            return httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For(restEaseType);
            };
        }

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
