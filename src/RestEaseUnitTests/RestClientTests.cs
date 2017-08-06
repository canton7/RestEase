using Moq;
using RestEase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests
{
    public class RestClientTests
    {
        public interface ISomeApi
        {
            [Get("foo")]
            Task FooAsync();
        }

        [Fact]
        public void NonGenericInstanceForReturnsSameAsGenericFor()
        {
            var generic = RestClient.For<ISomeApi>("http://example.com");
            var nonGeneric = new RestClient("http://example.com").For(typeof(ISomeApi));
            Assert.Equal(generic.GetType(), nonGeneric.GetType());
        }

        [Fact]
        public void NonGenericStaticForReturnsSameAsGenericFor()
        {
            var requester = new Mock<IRequester>().Object;
            var generic = RestClient.For<ISomeApi>("http://example.com");
            var nonGeneric = RestClient.For(typeof(ISomeApi), requester);
            Assert.Equal(generic.GetType(), nonGeneric.GetType());
        }
    }
}
