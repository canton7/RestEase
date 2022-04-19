using Moq;
using RestEase.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class GenericsTests : ImplementationFactoryTestsBase
    {
        public interface IHasGenericMethod
        {
            [Get("foo")]
            Task Foo<T>();
        }

        public interface IHasGenericReturnType
        {
            [Get("foo")]
            Task<T> Foo<T>();
        }

        public interface IHasGenericResponseReturnType
        {
            [Get("foo")]
            Task<Response<T>> Foo<T>();
        }

        public interface IHasGenericParameter
        {
            [Get]
            Task Foo<T>(T param);
        }

        public interface IHasGenericParameterConstrainedToEnumerable
        {
            [Get]
            Task Foo<T>(T param) where T : IEnumerable<string>;
        }

        public interface IHasGenericParameters
        {
            [Get("foo/{a}")]
            Task Foo<T1, T2>([Path] T1 a, [Query] T1 b, [Query] T2 c, [Query] IEnumerable<T1> d);
        }

        public class Base { }
        public interface IInterface { }
        public interface IHasGenericConstraint
        {
            [Get("foo")]
            Task Foo<T>() where T : Base, IInterface, new();
        }

        public interface IGenericType<T>
        {
            [Get("foo")]
            Task<T> FooAsync();
        }

        public interface IGenericTypeWithConstraints<T> where T : struct, IEquatable<T>
        {
            [Get]
            Task FooAsync(T param);
        }

        public interface IGenericTypeWithMultipleConstraints<T1, T2> where T1 : IEquatable<T1>, new() where T2 : IEnumerable<T1>
        {
            [Get]
            Task FooAsync(T1 foo, T2 bar);
        }

        public interface IGenericTypeWithVariance<in T>
        {
            [Get]
            Task FooAsync(T param);
        }

        public GenericsTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void SupportsGenericMethods()
        {
            this.Request<IHasGenericMethod>(x => x.Foo<string>());
        }

        [Fact]
        public void SupportsGenericReturnType()
        {
            // This asserts that the response type is as requested
            this.Request<IHasGenericReturnType, string>(x => x.Foo<string>(), "blah");
        }

        [Fact]
        public void SupportsGenericResponseReturnType()
        {
            var response = new Response<string>("blah", null, null);

            // This asserts that the response type is as requested
            this.RequestWithResponse<IHasGenericResponseReturnType, string>(x => x.Foo<string>(), response);
        }

        [Fact]
        public void SerializesGenericCollectionNoConstraint()
        {
            var requestInfo = this.Request<IHasGenericParameter>(x => x.Foo(new List<string>() { "foo", "bar" }));

            Assert.Single(requestInfo.QueryParams);
            var serialized = requestInfo.QueryParams.First().SerializeToString(null).ToList();

            Assert.Single(serialized);
            Assert.Equal("System.Collections.Generic.List`1[System.String]", serialized[0].Value);
        }

        [Fact]
        public void SerializesGenericCollectionWithConstraint()
        {
            var requestInfo = this.Request<IHasGenericParameterConstrainedToEnumerable>(x => x.Foo(new List<string>() { "foo", "bar" }));

            Assert.Single(requestInfo.QueryParams);
            var serialized = requestInfo.QueryParams.First().SerializeToString(null).ToList();

            Assert.Equal(2, serialized.Count);
            Assert.Equal("foo", serialized[0].Value);
            Assert.Equal("bar", serialized[1].Value);
        }

        [Fact]
        public void SupportsGenericMethodParameters()
        {
            var requestInfo = this.Request<IHasGenericParameters>(x => x.Foo("foo", "bar", new[] { "a", "b" }, new[] { "c", "d" }));

            var pathParams = requestInfo.PathParams.ToArray();
            Assert.Single(pathParams);
            Assert.Equal("a", pathParams[0].SerializeToString(null).Key);
            Assert.Equal("foo", pathParams[0].SerializeToString(null).Value);

            var queryParams = requestInfo.QueryParams.ToArray();
            Assert.Equal(3, queryParams.Length);

            Assert.Equal("b", queryParams[0].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("bar", queryParams[0].SerializeToString(null).ToArray()[0].Value);

            Assert.Equal("c", queryParams[1].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("System.String[]", queryParams[1].SerializeToString(null).ToArray()[0].Value);

            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[0].Key);
            Assert.Equal("c", queryParams[2].SerializeToString(null).ToArray()[0].Value);
            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[1].Key);
            Assert.Equal("d", queryParams[2].SerializeToString(null).ToArray()[1].Value);
        }

        [Fact]
        public void SupportsGenericConstraints()
        {
            var implementation = this.CreateImplementation<IHasGenericConstraint>();
            var methodInfo = implementation.GetType().GetTypeInfo().DeclaredMethods.Single(x => x.Name == "Foo");
            var constraints = methodInfo.GetGenericArguments()[0].GetGenericParameterConstraints();
            Assert.Equal(2, constraints.Length);
            Assert.Contains(typeof(Base), constraints);
            Assert.Contains(typeof(IInterface), constraints);
            Assert.Equal(GenericParameterAttributes.DefaultConstructorConstraint, methodInfo.GetGenericArguments()[0].GetGenericParameterAttributes());
        }


        [Fact]
        public void AllowsGenericApis()
        {
            this.Requester.Setup(x => x.RequestAsync<int>(It.IsAny<IRequestInfo>()))
                .Returns(Task.FromResult(3));

            var implementation = this.CreateImplementation<IGenericType<int>>();
            int result = implementation.FooAsync().Result;

            Assert.Equal(3, result);
        }

        [Fact]
        public void SupportsClassTypeConstraints()
        {
            var requestInfo = this.Request<IGenericTypeWithConstraints<double>>(x => x.FooAsync(3.0));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal("3", requestInfo.QueryParams.First().SerializeToString(null).First().Value);
        }

        [Fact]
        public void SupportsMultipleLevelsOfClassConstraints()
        {
            // TODO: Should this serialize as a collection or not? Currently a mismatch between the two emitters.

            var requestInfo = this.Request<IGenericTypeWithMultipleConstraints<Equatable, List<Equatable>>>(
                x => x.FooAsync(new Equatable("a"), new List<Equatable>() { new Equatable("b"), new Equatable("c") }));

            var queryParams = requestInfo.QueryParams.ToList();
            Assert.Equal(2, queryParams.Count);
            Assert.Equal("a", queryParams[0].SerializeToString(null).First().Value);

            var second = queryParams[1].SerializeToString(null).ToList();
            Assert.Equal(2, second.Count);
            Assert.Equal("b", second[0].Value);
            Assert.Equal("c", second[1].Value);
        }

        [Fact]
        public void SupportsClassTypeVariance()
        {
            var requestInfo = this.Request<IGenericTypeWithVariance<string>>(x => x.FooAsync("hello"));

            Assert.Single(requestInfo.QueryParams);
            Assert.Equal("hello", requestInfo.QueryParams.First().SerializeToString(null).First().Value);
        }

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>
        public class Equatable : IEquatable<Equatable>
        {
            private readonly string s;

            public Equatable() { }
            public Equatable(string s) => this.s = s;
            public bool Equals(Equatable other) => throw new NotImplementedException();
            public override string ToString() => this.s;
        }
#pragma warning restore CA1067 // Override Object.Equals(object) when implementing IEquatable<T>
    }
}
