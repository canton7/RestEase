using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using RestEase.Platform;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper methods called from generated code
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ImplementationHelpers
    {
#if !NETSTANDARD1_1
        /// <summary>
        /// Internal method. Do not call.
        /// </summary>
        public static MethodInfo GetInterfaceMethodInfo(MethodBase currentMethod, Type interfaceType)
        {
            var map = currentMethod.DeclaringType.GetInterfaceMap(interfaceType);
            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.TargetMethods[i] == currentMethod)
                {
                    return map.InterfaceMethods[i];
                }
            }

            throw new ImplementationCreationException($"Could not find interface type for {currentMethod.DeclaringType}.{currentMethod.Name} on {interfaceType.Name}. This is a bug");
        }
#endif

        /// <summary>
        /// Internal method. Do not call.
        /// </summary>
        public static MethodInfo GetInterfaceMethodInfo<TInterface, TReturn>(
            Expression<Func<TInterface, TReturn>> expr)
        {
            var methodInfo = ((MethodCallExpression)expr.Body).Method;
            return methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;
        }
    }
}
