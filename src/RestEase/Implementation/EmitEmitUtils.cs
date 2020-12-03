using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using RestEase.Platform;

namespace RestEase.Implementation
{
    internal static class EmitEmitUtils
    {
        public static void AddGenericTypeConstraints(Type[] genericArguments, GenericTypeParameterBuilder[] builders)
        {
            for (int i = 0; i < genericArguments.Length; i++)
            {
                var genericArgumentType = genericArguments[i].GetTypeInfo();
                var constraints = genericArgumentType.GetGenericParameterConstraints().Select(x => x.GetTypeInfo()).ToList();
                // We're generating a class: we can't have variance
                var attributes = genericArgumentType.GenericParameterAttributes & ~GenericParameterAttributes.VarianceMask;
                builders[i].SetGenericParameterAttributes(attributes);
                var baseType = constraints.FirstOrDefault(x => x.IsClass);
                if (baseType != null)
                {
                    builders[i].SetBaseTypeConstraint(baseType.AsType());
                }
                var interfaceTypes = constraints.Where(x => !x.IsClass).Select(x => x.AsType()).ToArray();
                if (interfaceTypes.Length > 0)
                {
                    builders[i].SetInterfaceConstraints(interfaceTypes);
                }
            }
        }

        public static string FriendlyNameForType(Type type)
        {
            var sb = new StringBuilder();
            Impl(type.GetTypeInfo());
            return sb.ToString();

            void Impl(TypeInfo typeInfo)
            {
                if (typeInfo.IsGenericType)
                {
                    string fullName = type.GetGenericTypeDefinition().FullName!;
                    sb.Append(fullName.Substring(0, fullName.LastIndexOf('`')));
                    sb.Append('<');
                    int i = 0;
                    foreach (var arg in typeInfo.GetGenericArguments())
                    {
                        if (i > 0)
                        {
                            sb.Append(',');
                        }
                        Impl(arg.GetTypeInfo());
                        i++;
                    }
                    sb.Append('>');
                }
                else
                {
                    sb.Append(typeInfo.FullName);
                }
            }
        }
    }
}
