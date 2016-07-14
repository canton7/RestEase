using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RestEase.Implementation
{
    internal static class CustomAttributeExtensions
    {
#if NET40
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(type, typeof(T));
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), false).Cast<T>();
        }

        public static T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(element, typeof(T));
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element)
        {
            return element.GetCustomAttributes(typeof(T), false).Cast<T>();
        }

        public static T GetCustomAttribute<T>(this ParameterInfo parameterInfo) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(parameterInfo, typeof(T));
        }

        public static Type AsType(this Type type)
        {
            return type;
        }

        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static Type CreateTypeInfo(this TypeBuilder builder)
        {
            return builder.CreateType();
        }
#endif
    }
}
