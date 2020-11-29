using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RestEase.Platform;

namespace RestEase.Implementation
{
    /// <summary>
    /// INTERNAL TYPE! This type may break between minor releases. Use at your own risk!
    /// 
    /// Helper to iterate both IDictionary and IDictionary{TKey, TValue} instances, as if both were IEnumerable{KeyValuePair{object, object}}
    /// </summary>
    public static class DictionaryIterator
    {
#if NETSTANDARD
        private static readonly MethodInfo iterateGenericTypedMethod = typeof(DictionaryIterator).GetTypeInfo().GetDeclaredMethod("IterateGenericTyped");
#else
        private static readonly MethodInfo iterateGenericTypedMethod = typeof(DictionaryIterator).GetMethod("IterateGenericTyped", BindingFlags.NonPublic | BindingFlags.Static)!;
#endif

        /// <summary>
        /// Returns true if we're capable of iterating the supplied type
        /// </summary>
        /// <param name="dictionaryType">Type to check</param>
        /// <returns>True if we're capable of iterating it</returns>
        public static bool CanIterate(Type dictionaryType)
        {
            var dictionaryTypeInfo = dictionaryType.GetTypeInfo();
            return typeof(IDictionary).GetTypeInfo().IsAssignableFrom(dictionaryTypeInfo) ||
                (dictionaryTypeInfo.IsGenericType && dictionaryType.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ||
                dictionaryTypeInfo.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        /// <summary>
        /// Iterates the given IDictionary or IDictionary{TKey, TValue} as if it was an IEnumerable{KeyValuePair{object, object}}
        /// </summary>
        /// <param name="dictionary">Dictionary to iterate</param>
        /// <returns>The equivalent IEnumerable{KeyValuePair{object, object}}</returns>
        public static IEnumerable<KeyValuePair<object?, object?>> Iterate(object dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary is IDictionary nonGeneric)
                return IterateNonGeneric(nonGeneric);

            // 'dictionary' cannot be an interface, so we're safe skipping to see whether
            // dictionary.GetType().GetGenericTypeDefinition() == IDictionary<,>
            foreach (var interfaceType in dictionary.GetType().GetTypeInfo().GetInterfaces())
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
                if (interfaceTypeInfo.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return IterateGeneric(dictionary, interfaceType);
            }

            throw new ArgumentException("Dictionary does not implement IDictionary or IDictionary<TKey, TValue>", nameof(dictionary));
        }

        private static IEnumerable<KeyValuePair<object?, object?>> IterateNonGeneric(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return new KeyValuePair<object?, object?>(entry.Key, entry.Value);
            }
        }

        private static IEnumerable<KeyValuePair<object?, object?>> IterateGeneric(object dictionary, Type dictionaryType)
        {
            var genericArguments = dictionaryType.GetTypeInfo().GetGenericArguments();
            var keyType = genericArguments[0];
            var valueType = genericArguments[1];

            var method = iterateGenericTypedMethod.MakeGenericMethod(keyType, valueType);
            return (IEnumerable<KeyValuePair<object?, object?>>)method.Invoke(null, new[] { dictionary })!;
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used via reflection")]
        private static IEnumerable<KeyValuePair<object?, object?>> IterateGenericTyped<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                yield return new KeyValuePair<object?, object?>(kvp.Key, kvp.Value);
            }
        }
    }
}
