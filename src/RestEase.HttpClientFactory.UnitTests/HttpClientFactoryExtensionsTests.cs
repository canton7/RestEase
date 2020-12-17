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

namespace RestEase.UnitTests.HttpClientFactoryTests
{
    public class HttpClientFactoryExtensionsTests
    {
        public interface ISomeApi
        {
            [Get]
            Task FooAsync();
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
    }
}
