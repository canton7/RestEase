using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
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

        public enum SomeEnum
        {
            A, B, C
        }

        public interface IHasEnum<T1> where T1 : class
        {
            [Get]
            Task FooAsync<T2>(SomeEnum e = SomeEnum.A) where T2 : struct;
        }

        public interface IHasDefaultEnum
        {
            [Get]
            Task FooAsync(SomeEnum e = default);
        }

        public interface IHasEnumOutOfRange
        {
            [Get]
            Task FooAsync(SomeEnum e = (SomeEnum)100);
        }

        public interface IHasDefaultsInExplicitImplementationBase
        {
            [Get]
            Task FooAsync(int foo = 3);
        }
        public interface IHasDefaultsInExplicitImplementation : IHasDefaultsInExplicitImplementationBase
        {
            [Get]
            new Task FooAsync(int foo = 4);
        }

        public DefaultValueTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void PreservesParameterNamesAndDefaultValues()
        {
            var implementation = this.CreateImplementation<IHasDefaultValue>();

            var methodInfo = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "GetFooAsync");
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

        [Fact]
        public void HandlesEnums()
        {
            var implementation = this.CreateImplementation<IHasEnum<string>>();

            var methodInfo = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "FooAsync");
            var parameters = methodInfo.GetParameters();

            Assert.Equal("e", parameters[0].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[0].Attributes);
            Assert.Equal(SomeEnum.A, parameters[0].DefaultValue);
        }

        [Fact]
        public void HandlesDefaultEnums()
        {
            var implementation = this.CreateImplementation<IHasDefaultEnum>();

            var methodInfo = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "FooAsync");
            var parameters = methodInfo.GetParameters();

            Assert.Equal("e", parameters[0].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[0].Attributes);
            Assert.Equal(SomeEnum.A, parameters[0].DefaultValue);
        }

        [Fact]
        public void HandlesOutOfRangeEnums()
        {
            var implementation = this.CreateImplementation<IHasEnumOutOfRange>();

            var methodInfo = implementation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "FooAsync");
            var parameters = methodInfo.GetParameters();

            Assert.Equal("e", parameters[0].Name);
            Assert.Equal(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameters[0].Attributes);
            Assert.Equal((SomeEnum)100, parameters[0].DefaultValue);
        }

        [Fact]
        public void HandlesDefaultsInExplicitImplementation()
        {
            // We're mainly checking that we don't get compiler warnings with the source generator
            var implementation = this.CreateImplementation<IHasDefaultsInExplicitImplementation>();

            var requestInfo = this.Request(implementation, x => x.FooAsync());
            Assert.Equal("4", requestInfo.QueryParams.Single().SerializeToString(null).Single().Value);

            requestInfo = this.Request(implementation, x => ((IHasDefaultsInExplicitImplementationBase)x).FooAsync());
            Assert.Equal("3", requestInfo.QueryParams.Single().SerializeToString(null).Single().Value);
        }
    }
}
