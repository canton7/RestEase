using System;
using System.ComponentModel;
using System.Reflection;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper methods called from generated code
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ImplementationHelpers
    {
        /// <summary>
        /// Internal method. Do not call.
        /// </summary>
        public static MethodInfo GetInterfaceMethodInfo(
            Type interfaceType,
            string name,
            int genericParameterCount,
            Type[] types)
        {
            // .NET Core has a nice GetMethod overload which takes genericParameterCount, but we don't (currently)
            // target that.
            var methods = interfaceType.GetTypeInfo().GetDeclaredMethods(name);
            foreach (var method in methods)
            {
                if (method.GetGenericArguments().Length != genericParameterCount)
                    continue;
                var parameters = method.GetParameters();
                if (parameters.Length != types.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != types[i])
                    {
                        match = false;
                        break;
                    }
                }
                if (!match)
                    continue;

                return method;
            }

            throw new ImplementationCreationException($"Unable to locate MethodInfo for {interfaceType}.{name}. This is a bug");
        }
    }
}
