using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RestEase.Platform
{
    internal static class TypeHelpers
    {
#if NETSTANDARD
        public static MethodInfo GetMethod(this TypeInfo typeInfo, string name)
        {
            return typeInfo.GetDeclaredMethod(name);
        }

        public static IEnumerable<MethodInfo> GetMethods(this TypeInfo typeInfo)
        {
            return typeInfo.DeclaredMethods;
        }

        public static PropertyInfo GetProperty(this TypeInfo typeInfo, string name)
        {
            return typeInfo.GetDeclaredProperty(name);
        }

        public static IEnumerable<PropertyInfo> GetProperties(this TypeInfo typeInfo)
        {
            return typeInfo.DeclaredProperties;
        }

        public static FieldInfo GetField(this TypeInfo typeInfo, string name)
        {
            return typeInfo.GetDeclaredField(name);
        }

        public static ConstructorInfo GetConstructor(this TypeInfo typeInfo, Type[] paramTypes)
        {
            return typeInfo.DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramTypes));
        }

        public static IEnumerable<Type> GetInterfaces(this TypeInfo typeInfo)
        {
            return typeInfo.ImplementedInterfaces;
        }

        public static IEnumerable<EventInfo> GetEvents(this TypeInfo typeInfo)
        {
            return typeInfo.DeclaredEvents;
        }

        public static Type[] GetGenericArguments(this TypeInfo typeInfo)
        {
            return typeInfo.GenericTypeArguments;
        }
#endif
    }
}
