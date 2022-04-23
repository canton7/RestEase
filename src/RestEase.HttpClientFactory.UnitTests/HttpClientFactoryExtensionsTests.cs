using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RestEase.HttpClientFactory;
using Moq;
using System.Net.Http;
using Moq.Protected;
using System.Threading;

namespace RestEase.UnitTests.HttpClientFactoryTests
{
    public class HttpClientFactoryExtensionsTests
    {
        public interface ISomeApi
        {
            [Get]
            Task FooAsync();
        }
        public interface ISomeApi2
        {
            [Get]
            Task FooAsync();
        }

        private class TestMessageHandler : HttpMessageHandler
        {
            public int CallCount { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.CallCount++;
                Assert.Equal("http://localhost/", request.RequestUri?.ToString());
                return Task.FromResult(new HttpResponseMessage());
            }
        }

        [Fact]
        public void RegistersGenericTransient()
        {
            var services = new ServiceCollection();

            services.AddRestEaseClient<ISomeApi>();

            var serviceProvider = services.BuildServiceProvider();

            var instance1 = serviceProvider.GetRequiredService<ISomeApi>();
            var instance2 = serviceProvider.GetRequiredService<ISomeApi>();
            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void RegistersNonGenericTransient()
        {
            var services = new ServiceCollection();

            services.AddRestEaseClient(typeof(ISomeApi));

            var serviceProvider = services.BuildServiceProvider();

            var instance1 = serviceProvider.GetRequiredService<ISomeApi>();
            var instance2 = serviceProvider.GetRequiredService<ISomeApi>();
            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void RegistersClientOnExistingHttpClientBuilder()
        {
            // Test that they resolve, and call the configured handler (and so use the HttpClient we created)

            var services = new ServiceCollection();

            var handler = new TestMessageHandler();

            services.AddHttpClient("test")
                .ConfigureHttpClient(x => x.BaseAddress = new Uri("http://localhost"))
                .ConfigurePrimaryHttpMessageHandler(() => handler)
                .UseWithRestEaseClient<ISomeApi>()
                .UseWithRestEaseClient<ISomeApi2>();

            var serviceProvider = services.BuildServiceProvider();

            var instance1 = serviceProvider.GetRequiredService<ISomeApi>();
            var instance2 = serviceProvider.GetRequiredService<ISomeApi2>();

            instance1.FooAsync().Wait();
            instance2.FooAsync().Wait();

            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public void RegistersRequestModifierAndPrimaryHandler()
        {
            var services = new ServiceCollection();

            int callCount = 0;
            var handler = new TestMessageHandler();

            services.AddRestEaseClient<ISomeApi>(
                requestModifier: (request, cancellationToken) =>
                {
                    callCount++;
                    return Task.CompletedTask;
                })
                .ConfigureHttpClient(x => x.BaseAddress = new Uri("http://localhost"))
                .ConfigurePrimaryHttpMessageHandler(() => handler);

            var serviceProvider = services.BuildServiceProvider();

            var instance = serviceProvider.GetRequiredService<ISomeApi>();
            instance.FooAsync().Wait();

            Assert.Equal(1, callCount);
            Assert.Equal(1, handler.CallCount);
        }
    }
}
