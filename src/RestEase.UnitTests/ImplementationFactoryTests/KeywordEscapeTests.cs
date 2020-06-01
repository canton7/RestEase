using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestEase.UnitTests.ImplementationFactoryTests.KeywordEscapeTestsNamespace.@event;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable IDE1006 // Naming Styles
namespace RestEase.UnitTests.ImplementationFactoryTests.KeywordEscapeTestsNamespace.@event
#pragma warning restore IDE1006 // Naming Styles
{
    [Header("Foo", "Bar")]
    public interface IApiInKeywordNamespace
    {
        [Get]
        Task FooAsync();
    }
}

namespace RestEase.UnitTests.ImplementationFactoryTests
{
    public class KeywordEscapeTests : ImplementationFactoryTestsBase
    {
        [Header("Foo", "Bar")]
#pragma warning disable IDE1006 // Naming Styles
        public interface @event
#pragma warning restore IDE1006 // Naming Styles
        {
            [Get]
            Task FooAsync();
        }

        [Header("Foo", "Bar")]
#pragma warning disable IDE1006 // Naming Styles
        public interface IKeywordInterfaceTypeParameter<@event>
#pragma warning restore IDE1006 // Naming Styles
        {
            [Query]
            @event Foo { get; set; }

            [Get]
            Task<@event> FooAsync(@event foo);
        }

        [Header("Foo", "Bar")]
        public interface IKeywordMethodTypeParameter
        {

            [Get]
#pragma warning disable IDE1006 // Naming Styles
            Task<@event> FooAsync<@event>(@event foo);
#pragma warning restore IDE1006 // Naming Styles
        }

        public interface IKeywordPropertyName
        {
            [Query]
#pragma warning disable IDE1006 // Naming Styles
            string @event { get; set; }
#pragma warning restore IDE1006 // Naming Styles

            [Get]
            Task FooAsync();
        }

        public interface IKeywordMethodName
        {
            [Get]
#pragma warning disable IDE1006 // Naming Styles
            Task @event();
#pragma warning restore IDE1006 // Naming Styles
        }

        public interface IKeywordParameterName
        {
            [Get]
            Task FooAsync(string @event);
        }

        public static class KeywordTypeTests
        {
#pragma warning disable IDE1006 // Naming Styles
            public class @event
            {
                public override string ToString() => "ToString";
            }
#pragma warning restore IDE1006 // Naming Styles

            public interface IKeywordPropertyType
            {
                [Query]
#pragma warning disable IDE1006 // Naming Styles
                @event @event { get; set; }
#pragma warning restore IDE1006 // Naming Styles

                [Get]
                Task FooAsync();
            }

            public interface IKeywordReturnType
            {
                [Get]
                Task<@event> FooAsync();
            }

            public interface IKeywordParameterType
            {
                [Get]
                Task FooAsync(@event @event);
            }

            public interface IKeywordTypeConstraint<T> where T : @event
            {
                [Get]
                Task FooAsync(T arg);
            }

            public interface IKeywordMethodTypeConstraint
            {
                [Get]
                Task FooAsync<T>(T arg) where T : @event;
            }
        }

        public KeywordEscapeTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void HandlesTypeInKeywordNamespace()
        {
            this.Request<IApiInKeywordNamespace>(x => x.FooAsync());
        }

        [Fact]
        public void HandlesKeywordInterfaceName()
        {
            this.Request<@event>(x => x.FooAsync());
        }

        [Fact]
        public void HandlesKeywordInterfaceTypeParameter()
        {
            this.Request<IKeywordInterfaceTypeParameter<string>, string>(x => x.FooAsync("yo"), "hello");
        }

        [Fact]
        public void HandlesKeywordMethodTypeParameter()
        {
            this.Request<IKeywordMethodTypeParameter, string>(x => x.FooAsync<string>("yo"), "hello");
        }

        [Fact]
        public void HandlesKeywordPropertyName()
        {
            var requestInfo = this.Request<IKeywordPropertyName>(x =>
            {
                x.@event = "test";
                return x.FooAsync();
            });

            Assert.Single(requestInfo.QueryProperties);
            var serialized = requestInfo.QueryProperties.First().SerializeToString(null).ToList();
            Assert.Equal("event", serialized[0].Key);
            Assert.Equal("test", serialized[0].Value);
        }

        [Fact]
        public void HandlesKeywordMethodName()
        {
            this.Request<IKeywordMethodName>(x => x.@event());
        }

        [Fact]
        public void HandlesKeywordParameterName()
        {
            var requestInfo = this.Request<IKeywordParameterName>(x => x.FooAsync("test"));

            Assert.Single(requestInfo.QueryParams);
            var serialized = requestInfo.QueryParams.First().SerializeToString(null).ToList();
            Assert.Equal("event", serialized[0].Key);
            Assert.Equal("test", serialized[0].Value);
        }

        [Fact]
        public void HandlesKeywordPropertyType()
        {
            var requestInfo = this.Request<KeywordTypeTests.IKeywordPropertyType>(x =>
            {
                x.@event = new KeywordTypeTests.@event();
                return x.FooAsync();
            });

            Assert.Single(requestInfo.QueryProperties);
            var serialized = requestInfo.QueryProperties.First().SerializeToString(null).ToList();
            Assert.Equal("event", serialized[0].Key);
            Assert.Equal("ToString", serialized[0].Value);
        }

        [Fact]
        public void HandlesKeywordReturnType()
        {
            this.Request<KeywordTypeTests.IKeywordReturnType, KeywordTypeTests.@event>(x => x.FooAsync(), new KeywordTypeTests.@event());
        }

        [Fact]
        public void HandlesKeywordParametertype()
        {
            var requestInfo = this.Request<KeywordTypeTests.IKeywordParameterType>(x => x.FooAsync(new KeywordTypeTests.@event()));

            Assert.Single(requestInfo.QueryParams);
            var serialized = requestInfo.QueryParams.First().SerializeToString(null).ToList();
            Assert.Equal("event", serialized[0].Key);
            Assert.Equal("ToString", serialized[0].Value);
        }

        [Fact]
        public void HandlesKeywordTypeConstraint()
        {
            this.Request<KeywordTypeTests.IKeywordTypeConstraint<KeywordTypeTests.@event>>(x => x.FooAsync(new KeywordTypeTests.@event()));
        }

        [Fact]
        public void HandlesKeywordMethodtypeConstraint()
        {
            this.Request<KeywordTypeTests.IKeywordMethodTypeConstraint>(x => x.FooAsync<KeywordTypeTests.@event>(new KeywordTypeTests.@event()));
        }
    }
}
