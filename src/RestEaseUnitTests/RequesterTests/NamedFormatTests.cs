using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestEaseUnitTests.RequesterTests
{
    public class NamedFormatTests
    {
        [Fact]
        public void FindsBothArgumentsWithNoFormat()
        {
            NamedFormat format = new NamedFormat("/path/{foo}/{bar}");
            var args = format.GetArguments().ToList();
            Assert.Equal(2, args.Count);
            Assert.Equal("foo", args[0].Name);
            Assert.False(args[0].HasFormat());
            Assert.Equal("bar", args[1].Name);
            Assert.False(args[1].HasFormat());
        }

        [Fact]
        public void FindsBothArgumentsOneWithFormat()
        {
            NamedFormat format = new NamedFormat("/path/{foo:FORMAT}/{bar}");
            var args = format.GetArguments().ToList();
            Assert.Equal(2, args.Count);
            Assert.Equal("foo", args[0].Name);
            Assert.Equal("FORMAT", args[0].Format);
            Assert.Equal("bar", args[1].Name);
            Assert.False(args[1].HasFormat());
        }

        [Fact]
        public void ArgumentWithNoFormatIsNull()
        {
            NamedFormat format = new NamedFormat("/path/{foo}");
            var args = format.GetArguments().ToList();
            Assert.Equal(1, args.Count);
            Assert.False(args[0].HasFormat());
            Assert.Null(args[0].Format);
        }
        
        [Fact]
        public void SameArgumentWithMultipleFormats()
        {
            NamedFormat format = new NamedFormat("/path/{foo:ONE}/{foo:TWO}/{foo:THREE}");
            var args = format.GetArguments().ToList();
            Assert.Equal(3, args.Count);
            Assert.Equal("foo", args[0].Name);
            Assert.Equal("ONE", args[0].Format);
            Assert.Equal("foo", args[1].Name);
            Assert.Equal("TWO", args[1].Format);
            Assert.Equal("foo", args[2].Name);
            Assert.Equal("THREE", args[2].Format);
        }

        [Fact]
        public void ReplaceWithFormats()
        {
            Guid guid = Guid.Parse("28ac07a0-be1c-4dc9-b583-89a95e789c12");
            DateTime christmas = new DateTime(2016, 12, 24, 23, 59, 59);
            var values = new Dictionary<string, object>() {
                { "foo", guid },
                { "bar", christmas },
                { "baz", 15 },
            };
            
            NamedFormat format = new NamedFormat("{foo:N}/{bar:yyyyMMdd}/{baz:D4}");
            var result = format.ReplaceWith(values).StringValue();

            Assert.Equal("28ac07a0be1c4dc9b58389a95e789c12/20161224/0015", result);
        }

        [Fact]
        public void ReplaceWithoutFormats()
        {
            Guid guid = Guid.Parse("28ac07a0-be1c-4dc9-b583-89a95e789c12");
            DateTime christmas = new DateTime(2016, 12, 24, 23, 59, 59);
            var values = new Dictionary<string, object>() {
                { "foo", guid },
                { "bar", "some-string" },
                { "baz", 324 },
            };

            NamedFormat format = new NamedFormat("{foo}/{bar}/{baz}");
            var result = format.ReplaceWith(values).StringValue();

            Assert.Equal("28ac07a0-be1c-4dc9-b583-89a95e789c12/some-string/324", result);
        }
                
        [Fact]
        public void ReplaceWorksWithMultipleFormatsOnSameValue()
        {
            Guid guid = Guid.Parse("28ac07a0-be1c-4dc9-b583-89a95e789c12");
            var values = new Dictionary<string, object>() {
                { "foo", guid }
            };

            NamedFormat format = new NamedFormat("{foo}/{foo:N}");
            var result = format.ReplaceWith(values).StringValue();

            Assert.Equal("28ac07a0-be1c-4dc9-b583-89a95e789c12/28ac07a0be1c4dc9b58389a95e789c12", result);
        }

        [Fact]
        public void ReplaceWillReplaceWithEmptyStringIfVariableIsUndefined()
        {
            var values = new Dictionary<string, object>() {
                { "foo", Guid.NewGuid() }
            };

            NamedFormat format = new NamedFormat("before/{bar}/after");
            var result = format.ReplaceWith(values).StringValue();

            Assert.Equal("before//after", result);
        }

        [Fact]
        public void ReplaceTransformWillUpdateStringValue()
        {
            var values = new Dictionary<string, object>() {
                { "foo", Guid.NewGuid() }
            };

            NamedFormat format = new NamedFormat("/{foo}");
            var result = format.ReplaceWith(values)
                               .WithTransform(s => string.Empty)
                               .StringValue();

            Assert.Equal("/", result);
        }

        public void ReplaceTransformWillAlwasyBeApplied()
        {
            var values = new Dictionary<string, object>() {
                { "foo", Guid.NewGuid() }
            };

            NamedFormat format = new NamedFormat("/{foo}");
            var result = format.ReplaceWith(values)
                               .WithTransform(s => "override")
                               .StringValue();

            Assert.Equal("/override", result);
        }
    }
}
