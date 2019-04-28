using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

#if !NETSTANDARD1_1
using System.ComponentModel;
using System.Runtime.Serialization;
#endif

namespace RestEase
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// A serializer that handles enum values specially, serializing them into their display value
    /// as defined by a Display or EnumMember attribute.
    /// </summary>
    public class StringEnumRequestPathParamSerializer : RequestPathParamSerializer
    {
        private static readonly ConcurrentDictionary<object, string> cache = new ConcurrentDictionary<object, string>();

        /// <inheritdoc />
        /// <summary>
        /// Serialize a path parameter whose value is scalar (not a collection), into a string value
        /// </summary>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A string value to use as path parameter</returns>
        /// <remarks>
        /// If the value is an enum value, the serializer will check if it has an EnumMember or Display
        /// attribute, and if so return the value of that instead.
        /// </remarks>
        public override string SerializePathParam<T>(T value, RequestPathParamSerializerInfo info)
        {
            var type = typeof(T);

            return IsEnum(type) ? GetEnumDisplayName(value) : value.ToString();
        }

        private static string GetEnumDisplayName<T>(T value)
        {
            var type = typeof(T);

            if (!IsEnum(type))
            {
                return value.ToString();
            }

            if (cache.TryGetValue(value, out var stringValue))
            {
                return stringValue;
            }

            stringValue = value.ToString();

            var fieldInfo = GetField(type, stringValue);

            if (fieldInfo == null)
            {
                return CacheAdd(value, stringValue);
            }

#if !NETSTANDARD1_1
            var enumMemberAttribute = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();

            if (enumMemberAttribute != null)
            {
                return CacheAdd(value, enumMemberAttribute.Value);
            }

            var displayNameAttribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();

            if (displayNameAttribute != null)
            {
                return CacheAdd(value, displayNameAttribute.DisplayName);
            }
#endif

            var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null)
            {
                return CacheAdd(value, displayAttribute.Name);
            }

            return CacheAdd(value, stringValue);
        }

        private static string CacheAdd(object key, string value)
        {
            cache.TryAdd(key, value);
            return value;
        }

        private static bool IsEnum(Type type)
        {
#if NETSTANDARD1_1
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        private static FieldInfo GetField(Type type, string name)
        {
#if NETSTANDARD1_1
            return type.GetTypeInfo().GetDeclaredField(name);
#else
            return type.GetField(name);
#endif
        }
    }
}
