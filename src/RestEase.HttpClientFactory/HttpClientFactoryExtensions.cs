using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

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
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(this IServiceCollection services, string? basePath, Action<RestClient>? configurer = null)
            where T : class
        {
            return services.AddHttpClient(UniqueNameForType(typeof(T)), httpClient =>
            {
                if (basePath != null)
                {
                    httpClient.BaseAddress = new Uri(basePath);
                }
            }).AddTypedClient(httpClient =>
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
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(this IServiceCollection services, Type restEaseType, string? basePath, Action<RestClient>? configurer = null)
        {
            return services.AddHttpClient(UniqueNameForType(restEaseType), httpClient =>
            {
                if (basePath != null)
                {
                    httpClient.BaseAddress = new Uri(basePath);
                }
            }).AddTypedClient(httpClient =>
            {
                var restClient = new RestClient(httpClient);
                configurer?.Invoke(restClient);
                return restClient.For(restEaseType);
            });
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
