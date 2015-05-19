using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper to iterate both IDictionary and IDictionary{TKey, TValue} instances, as if both were IEnumerable{KeyValuePair{object, object}}
    /// </summary>
    public static class DictionaryIterator
    {
        private static readonly MethodInfo iterateGenericTypedMethod = typeof(DictionaryIterator).GetMethod("IterateGenericTyped", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Returns true if we're capable of iterating the supplied type
        /// </summary>
        /// <param name="dictionaryType">Type to check</param>
        /// <returns>True if we're capable of iterating it</returns>
        public static bool CanIterate(Type dictionaryType)
        {
            return typeof(IDictionary).IsAssignableFrom(dictionaryType) ||
                (dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ||
                dictionaryType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        /// <summary>
        /// Iterates the given IDictionary or IDictionary{TKey, TValue} as if it was an IEnumerable{KeyValuePair{object, object}}
        /// </summary>
        /// <param name="dictionary">Dictionary to iterate</param>
        /// <returns>The equivalent IEnumerable{KeyValuePair{object, object}}</returns>
        public static IEnumerable<KeyValuePair<object, object>> Iterate(object dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (dictionary is IDictionary)
                return IterateNonGeneric((IDictionary)dictionary);

            // 'dictionary' cannot be an interface, so we're safe skipping to see whether
            // dictionary.GetType().GetGenericTypeDefinition() == IDictionary<,>
            foreach (var interfaceType in dictionary.GetType().GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return IterateGeneric(dictionary, interfaceType);
            }

            throw new ArgumentException("Dictionary does not implement IDictionary or IDictionary<TKey, TValue>");
        }

        private static IEnumerable<KeyValuePair<object, object>> IterateNonGeneric(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> IterateGeneric(object dictionary, Type dictionaryType)
        {
            var genericArguments = dictionaryType.GetGenericArguments();
            var keyType = genericArguments[0];
            var valueType = genericArguments[1];

            var method = iterateGenericTypedMethod.MakeGenericMethod(keyType, valueType);
            return (IEnumerable<KeyValuePair<object, object>>)method.Invoke(null, new[] { dictionary });
        }

        private static IEnumerable<KeyValuePair<object, object>> IterateGenericTyped<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                yield return new KeyValuePair<object, object>(kvp.Key, kvp.Value);
            }
        }
    }
}
