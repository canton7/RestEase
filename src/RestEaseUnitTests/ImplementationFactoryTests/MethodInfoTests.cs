using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class MethodInfoTests : ImplementationFactoryTestsBase
    {
        public interface IHasOverloads
        {
            [Get]
            Task FooAsync(string foo);

            [Get]
            Task FooAsync(int foo);
        }

        public interface IParent
        {
            [Get]
            Task FooAsync(string bar);
        }
        public interface IChild : IParent { }

        public interface IGenericParent<T>
        {
            [Get]
            Task FooAsync(T bar);
        }

        public interface IChildWithTwoGenericParents : IGenericParent<int>, IGenericParent<string> { }

        public interface IHasTwoMethodsWithDifferentArity
        {
            [Get]
            Task FooAsync();

            [Get]
            Task FooAsync<T>();
        }

        public interface IHasGenericParameter
        {
            [Get]
            Task FooAsync<T>(T arg);
        }

        public MethodInfoTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void GetsMethodInfoForOverloadedMethods()
        {
            var intOverload = this.Request<IHasOverloads>(x => x.FooAsync(1)).MethodInfo;
            var expectedIntOverload = typeof(IHasOverloads).GetTypeInfo().GetMethod("FooAsync", new Type[] { typeof(int) });
            Assert.Equal(expectedIntOverload, intOverload);

            var stringOverload = this.Request<IHasOverloads>(x => x.FooAsync("test")).MethodInfo;
            var expectedstringOverload = typeof(IHasOverloads).GetTypeInfo().GetMethod("FooAsync", new Type[] { typeof(string) });
            Assert.Equal(expectedstringOverload, stringOverload);
        }

        [Fact]
        public void GetsMethodInfoFromParentInterface()
        {
            var methodInfo = this.Request<IChild>(x => x.FooAsync("testy")).MethodInfo;
            var expected = typeof(IParent).GetTypeInfo().GetMethod("FooAsync");
            Assert.Equal(expected, methodInfo);
        }

        [Fact]
        public void GetsCorrectGenericMethod()
        {
            var intOverload = this.Request<IChildWithTwoGenericParents>(x => x.FooAsync(1)).MethodInfo;
            var expectedIntOverload = typeof(IGenericParent<int>).GetTypeInfo().GetMethod("FooAsync");
            Assert.Equal(expectedIntOverload, intOverload);

            var stringOverload = this.Request<IChildWithTwoGenericParents>(x => x.FooAsync("test")).MethodInfo;
            var expectedstringOverload = typeof(IGenericParent<string>).GetTypeInfo().GetMethod("FooAsync");
            Assert.Equal(expectedstringOverload, stringOverload);
        }

        [Fact]
        public void GetsMethodWithCorrectArity()
        {
            var zeroArity = this.Request<IHasTwoMethodsWithDifferentArity>(x => x.FooAsync()).MethodInfo;
            var expectedZeroArity = typeof(IHasTwoMethodsWithDifferentArity)
                .GetTypeInfo().GetDeclaredMethods("FooAsync").FirstOrDefault(x => x.GetGenericArguments().Length == 0);
            Assert.Equal(expectedZeroArity, zeroArity);

            var oneArity = this.Request<IHasTwoMethodsWithDifferentArity>(x => x.FooAsync<int>()).MethodInfo;
            var expectedOneArity = typeof(IHasTwoMethodsWithDifferentArity)
                .GetTypeInfo().GetDeclaredMethods("FooAsync").FirstOrDefault(x => x.GetGenericArguments().Length == 1);
            Assert.Equal(expectedOneArity, oneArity);

            Assert.NotEqual(zeroArity, oneArity);
        }

        [Fact]
        public void GetsMethodWithGenericParameter()
        {
            var methodInfo = this.Request<IHasGenericParameter>(x => x.FooAsync(3)).MethodInfo;
            var expected = typeof(IHasGenericParameter).GetTypeInfo().GetDeclaredMethods("FooAsync").Single();
            Assert.Equal(expected, methodInfo);
        }
    }
}
