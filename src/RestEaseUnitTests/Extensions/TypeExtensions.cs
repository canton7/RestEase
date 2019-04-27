
using System;

namespace RestEaseUnitTests.Extensions
{
    using System.Reflection;

    public static class TypeExtensions
    {
#if NETCOREAPP1_0
        public static Type[] GetGenericParameterConstraints(this Type type)
        {
            return type.GetTypeInfo().GetGenericParameterConstraints();
        }
#endif

        public static GenericParameterAttributes GetGenericParameterAttributes(this Type type)
        {
#if NETCOREAPP1_0
            return type.GetTypeInfo().GenericParameterAttributes;
#else
            return type.GenericParameterAttributes;
#endif
        }
    }
}
