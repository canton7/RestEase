using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RestEase.Implementation;
using Xunit;

namespace RestEase.UnitTests.RequesterTests
{
    public class DictionaryIteratorTests
    {
        [Fact]
        public void CanIterateIDictionary()
        {
            Assert.True(DictionaryIterator.CanIterate(typeof(IDictionary)));
        }

        [Fact]
        public void CanIterateIDictionarySubclass()
        {
            Assert.True(DictionaryIterator.CanIterate(typeof(Hashtable)));
        }

        [Fact]
        public void CanIterateIDictionaryKV()
        {
            Assert.True(DictionaryIterator.CanIterate(typeof(IDictionary<int, int>)));
        }

        [Fact]
        public void CanIterateIDictionaryKVSubclass()
        {
            Assert.True(DictionaryIterator.CanIterate(typeof(Dictionary<int, int>)));
        }

        [Fact]
        public void IteratesIDictionary()
        {
            var dict = new Hashtable()
            {
                { "k1", 1 },
                { "k2", "v2" }
            };
            var actual = DictionaryIterator.Iterate(dict).OrderBy(x => x.Key).ToList();
            var expected = new List<KeyValuePair<object, object>>()
            {
                new KeyValuePair<object, object>("k1", 1),
                new KeyValuePair<object, object>("k2", "v2"),
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IteratesIDictionaryKV()
        {
            dynamic dict = new ExpandoObject(); // Implements IDictionary<string, object>, but not IDictionary
            dict.k1 = 1;
            dict.k2 = "v2";
            var actual = DictionaryIterator.Iterate((object)dict).OrderBy(x => x.Key).ToList();
            var expected = new List<KeyValuePair<object, object>>()
            {
                new KeyValuePair<object, object>("k1", 1),
                new KeyValuePair<object, object>("k2", "v2"),
            };
            Assert.Equal(expected, actual);
        }
    }
}
