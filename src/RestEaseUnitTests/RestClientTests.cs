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
        public void NonGenericForReturnsSameAsGenericFor()
        {
            var generic = RestClient.For<ISomeApi>("http://example.com");
            var nonGeneric = new RestClient("http://example.com").For(typeof(ISomeApi));
            Assert.Equal(generic.GetType(), nonGeneric.GetType());
        }
    }
}
