using RestEase.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RestEase.Implementation
{
    /// <summary>
    /// Helper methods used by generated interface implementations. You should not call these directly
    /// </summary>
    public static class ImplementationHelpers
    {
        /// <summary>
        /// Internal method. Do not call.
        /// </summary>
        [DebuggerHidden]
        public static MethodInfo GetMethod(RuntimeTypeHandle typeHandle, string name, RuntimeTypeHandle[] parameterHandles)
        {
            // Avoid linq, as this is called at runtime rather than type-generation time

            var type = Type.GetTypeFromHandle(typeHandle);
            var methods = type.GetTypeInfo().GetMethods();

            MethodInfo foundMethod = null;

            foreach (var method in methods)
            {
                if (method.Name != name)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != parameterHandles.Length)
                    continue;

                bool parametersMatch = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].ParameterType.TypeHandle.Equals(parameterHandles[i]))
                    {
                        parametersMatch = false;
                        break;
                    }
                }

                if (!parametersMatch)
                    continue;

                foundMethod = method;
                break;
            }

            if (foundMethod == null)
                throw new ImplementationCreationException($"Could not find method {name} on type {Type.GetTypeFromHandle(typeHandle).Name} with parameters {string.Join(", ", parameterHandles.Select(x => Type.GetTypeFromHandle(x).Name))}. This should not happen!");

            return foundMethod;
        }
    }
}
