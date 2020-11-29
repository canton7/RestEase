using System.Collections.Concurrent;
using System.Reflection;
using RestEase.Implementation;
using System.Linq;
using RestEase.Platform;

#if !NETSTANDARD1_1
using System.ComponentModel;
using System.Runtime.Serialization;
#endif

namespace RestEase
{
    /// <inheritdoc />
    /// <summary>
    /// A serializer that handles enum values specially, serializing them into their display value
    /// as defined by a EnumMember, DisplayName, or Display attribute (in that order).
    /// </summary>
    public class StringEnumRequestPathParamSerializer : RequestPathParamSerializer
    {
        private static readonly ConcurrentDictionary<object, string> cache = new();

        /// <inheritdoc />
        /// <summary>
        /// Serialize a path parameter whose value is scalar (not a collection), into a string value
        /// </summary>
        /// <typeparam name="T">Type of the value to serialize</typeparam>
        /// <param name="value">Value of the path parameter</param>
        /// <param name="info">Extra info which may be useful to the serializer</param>
        /// <returns>A string value to use as path parameter</returns>
        /// <remarks>
        /// If the value is an enum value, the serializer will check if it has an EnumMember, DisplayName or Display
        /// attribute, and if so return the value of that instead (in that order of preference).
        /// </remarks>
        public override string? SerializePathParam<T>(T value, RequestPathParamSerializerInfo info)
        {
            if (value == null)
            {
                return null;
            }

            var typeInfo = typeof(T).GetTypeInfo();

            if (!typeInfo.IsEnum)
            {
                return ToStringHelper.ToString(value, info.Format, info.FormatProvider);
            }

            if (cache.TryGetValue(value, out string? stringValue))
            {
                return stringValue;
            }

            stringValue = value.ToString()!;

            var fieldInfo = typeInfo.GetField(stringValue);

            if (fieldInfo == null)
            {
                return CacheAdd(value, stringValue);
            }

#if !NETSTANDARD1_1
            var enumMemberAttribute = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();

            if (enumMemberAttribute?.Value != null)
            {
                return CacheAdd(value, enumMemberAttribute.Value);
            }

            var displayNameAttribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();

            if (displayNameAttribute != null)
            {
                return CacheAdd(value, displayNameAttribute.DisplayName);
            }
#endif

            // netstandard can get this by referencing System.ComponentModel.DataAnnotations, (and framework
            // can get this by referencing the assembly). However we don't want a dependency on this nuget package,
            // for something so niche, so do a reflection-only load
            var displayAttribute = fieldInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.ComponentModel.DataAnnotations.DisplayAttribute");
            if (displayAttribute != null)
            {
                object? name = displayAttribute.NamedArguments.FirstOrDefault(x => x.MemberName == "Name").TypedValue.Value;
                if (name != null)
                {
                    return CacheAdd(value, (string)name);
                }
            }

            return CacheAdd(value, stringValue);
        }

        private static string CacheAdd(object key, string value)
        {
            cache.TryAdd(key, value);
            return value;
        }
    }
}
