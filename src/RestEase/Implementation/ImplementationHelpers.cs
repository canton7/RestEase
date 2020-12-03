using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace RestEase.Implementation
{
    /// <summary>
    /// Internal type. Do not use! Helper methods called from generated code
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ImplementationHelpers
    {
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
