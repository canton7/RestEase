using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class DefaultValueTests
    {
        public interface IHasDefaultValue
        {
            [Get("foo")]
            Task GetFooAsync(string foo = "", int bar = 3);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = ImplementationBuilder.Instance;

        [Fact]
        public void PreservesParameterNamesAndDefaultValues()
        {
            var implementation = this.builder.CreateImplementation<IHasDefaultValue>(this.requester.Object);

            var methodInfo = implementation.GetType().GetMethod("GetFooAsync");
            var parameters = methodInfo.GetParameters();

            Assert.Equal("foo", parameters[0].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[0].Attributes);
            Assert.Equal(string.Empty, parameters[0].DefaultValue);

            Assert.Equal("bar", parameters[1].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[1].Attributes);
            Assert.Equal(3, parameters[1].DefaultValue);
        }

    }
}
