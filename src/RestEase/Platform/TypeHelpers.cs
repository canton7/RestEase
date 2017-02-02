using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RestEase.Platform
{
    internal static class TypeHelpers
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

        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assemblyName, AssemblyBuilderAccess access)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, access);
        }
#else
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assemblyName, AssemblyBuilderAccess access)
        {
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, access);
        }
#endif

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

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.SetMethod;
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
