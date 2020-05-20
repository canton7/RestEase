using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class DefaultValueTests : ImplementationFactoryTestsBase
    {
        public interface IHasDefaultValue
        {
            [Get("foo")]
            Task GetFooAsync(string foo = "", int bar = 3, float? baz = null);
        }

        public interface IHasDefaultCancellationToken
        {
            [Get("foo")]
            Task GetFooAsync(CancellationToken cancellationToken = default);
        }

        public DefaultValueTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void PreservesParameterNamesAndDefaultValues()
        {
            var implementation = this.CreateImplementation<IHasDefaultValue>();

            var methodInfo = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name.EndsWith(".GetFooAsync"));
            var parameters = methodInfo.GetParameters();

            Assert.Equal("foo", parameters[0].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[0].Attributes);
            Assert.Equal(string.Empty, parameters[0].DefaultValue);

            Assert.Equal("bar", parameters[1].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[1].Attributes);
            Assert.Equal(3, parameters[1].DefaultValue);

            Assert.Equal("baz", parameters[2].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[2].Attributes);
            Assert.Null(parameters[2].DefaultValue);
        }

        [Fact]
        public void HandlesDefaultStructValues()
        {
            this.CreateImplementation<IHasDefaultCancellationToken>();
        }

    }
}
