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
    public static partial class HttpClientFactoryExtensions
    {
        // Needed otherwise `AddRestEaseClient()` is ambiguous

        /// <summary>
        /// Register the given RestEase interface type, without a Base Address. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services)
            where T : class
        {
            return services.AddRestEaseClient<T>((string?)null, new());
        }

        // Needs to exist otherwise we can't call `AddRestEaseClient<T>(options)`

        /// <summary>
        /// Register the given RestEase interface type, without a Base Address. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            AddRestEaseClientOptions<T> options)
            where T : class
        {
            return services.AddRestEaseClient((string?)null, options);
        }

        // Needs to exist otherwise we can't call `AddRestEaseClient<T>(uri)`

        /// <summary>
        /// Register the given RestEase interface type
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="services">Contains to add to</param>
        /// /// <param name="baseAddress">
        /// Base address to use for requests (may be <c>null</c> if your interface specifies
        /// <see cref="BaseAddressAttribute"/>, has an absolute <see cref="BasePathAttribute"/>, or only uses absolute
        /// paths)
        /// </param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            string? baseAddress)
            where T : class
        {
            return services.AddRestEaseClient<T>(baseAddress, new());
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
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            string? baseAddress,
            AddRestEaseClientOptions<T> options) // Can't be nullable otherwise it's ambiguous with obsolete overloads
            where T : class
        {
            return services.AddRestEaseClient(ToUri(baseAddress), options);
        }

        // Needs to exist otherwise we can't call `AddRestEaseClient<T>(uri)`

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
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Uri? baseAddress)
            where T : class
        {
            return services.AddRestEaseClient<T>(baseAddress, new());
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
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient<T>(
            this IServiceCollection services,
            Uri? baseAddress,
            AddRestEaseClientOptions<T> options) // Can't be nullable otherwise it's ambiguous with obsolete overloads
            where T : class
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return services
                .CreateHttpClientBuilder(typeof(T), baseAddress)
                .AddRestEaseClientCore(
                    typeof(T),
                    GenericFactory(options.RequesterFactory, options.RestClientConfigurer, options.InstanceConfigurer),
                    options.RequestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <param name="services">Contains to add to</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType)
        {
            return services.AddRestEaseClient(restEaseType, (string?)null, new());
        }

        /// <summary>
        /// Register the given RestEase interface type. The interface should have an absolute
        /// <see cref="BaseAddressAttribute"/> or <see cref="BasePathAttribute"/>, or should only use absolute paths.
        /// </summary>
        /// <param name="services">Contains to add to</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            AddRestEaseClientOptions options) // Can't be nullable otherwise it's ambiguous with obsolete overloads
        {
            return services.AddRestEaseClient(restEaseType, (string?)null, options);
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
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            string? baseAddress)
        {
            return services.AddRestEaseClient(restEaseType, baseAddress, new());
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
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            string? baseAddress,
            AddRestEaseClientOptions options)
        {
            return services.AddRestEaseClient(restEaseType, ToUri(baseAddress), options);
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
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress)
        {
            return services.AddRestEaseClient(restEaseType, baseAddress, new());
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
        /// <param name="options">Additional options to pass</param>
        /// <returns>Created <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder AddRestEaseClient(
            this IServiceCollection services,
            Type restEaseType,
            Uri? baseAddress,
            AddRestEaseClientOptions options)
        {
            if (restEaseType is null)
                throw new ArgumentNullException(nameof(restEaseType));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return services
                .CreateHttpClientBuilder(restEaseType, baseAddress)
                .AddRestEaseClientCore(
                    restEaseType,
                    NonGenericFactory(restEaseType, options.RequesterFactory, options.RestClientConfigurer),
                    options.RequestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder UseWithRestEaseClient<T>(
            this IHttpClientBuilder httpClientBuilder)
            where T : class
        {
            return httpClientBuilder.UseWithRestEaseClient(new UseWithRestEaseClientOptions<T>());
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <typeparam name="T">Type of the RestEase interface</typeparam>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="options">Additional options to pass</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder UseWithRestEaseClient<T>(
            this IHttpClientBuilder httpClientBuilder,
            UseWithRestEaseClientOptions<T> options)
            where T : class
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return httpClientBuilder.AddRestEaseClientCore(
                typeof(T),
                GenericFactory(options.RequesterFactory, options.RestClientConfigurer, options.InstanceConfigurer),
                options.RequestModifier);
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder UseWithRestEaseClient(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType)
        {
            return httpClientBuilder.UseWithRestEaseClient(restEaseType, new UseWithRestEaseClientOptions());
        }

        /// <summary>
        /// Register the given RestEase interface type to use the a <see cref="HttpClient"/>
        /// from the given <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <param name="httpClientBuilder"><see cref="IHttpClientBuilder"/> which RestEase should use a <see cref="HttpClient"/> from</param>
        /// <param name="restEaseType">Type of the RestEase interface</param>
        /// <param name="options">Additional options to pass</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration</returns>
        public static IHttpClientBuilder UseWithRestEaseClient(
            this IHttpClientBuilder httpClientBuilder,
            Type restEaseType,
            UseWithRestEaseClientOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return httpClientBuilder.AddRestEaseClientCore(
                restEaseType,
                NonGenericFactory(restEaseType, options.RequesterFactory, options.RestClientConfigurer),
                options.RequestModifier);
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

        private static Func<HttpClient, object> GenericFactory<T>(
            Func<HttpClient, IRequester>? requesterFactory,
            Action<RestClient>? clientConfigurer,
            Action<T>? instanceConfigurer) where T : class
        {
            if (requesterFactory == null)
            {
                return httpClient =>
                {
                    var restClient = new RestClient(httpClient);
                    clientConfigurer?.Invoke(restClient);
                    var instance = restClient.For<T>();
                    instanceConfigurer?.Invoke(instance);
                    return instance;
                };
            }
            else
            {
                if (clientConfigurer != null)
                    throw new ArgumentException("If RequesterFactory is specified, RestClientConfigurer must be null");

                return httpClient =>
                {
                    var instance = RestClient.For<T>(requesterFactory(httpClient));
                    instanceConfigurer?.Invoke(instance);
                    return instance;
                };
            }
        }

        private static Func<HttpClient, object> NonGenericFactory(
            Type restEaseType,
            Func<HttpClient, IRequester>? requesterFactory,
            Action<RestClient>? clientConfigurer)
        {
            if (requesterFactory == null)
            {
                return httpClient =>
                {
                    var restClient = new RestClient(httpClient);
                    clientConfigurer?.Invoke(restClient);
                    return restClient.For(restEaseType);
                };
            }
            else
            {
                if (clientConfigurer != null)
                    throw new ArgumentException("If RequesterFactory is specified, RestClientConfigurer must be null");

                return httpClient =>
                {
                    return RestClient.For(restEaseType, requesterFactory(httpClient));
                };
            }
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
