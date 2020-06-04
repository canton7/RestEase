using System;
using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class MethodSignatureEqualityComparer
    {
        public bool Equals(MethodModel x, MethodModel y)
        {
            var xSymbol = x?.MethodSymbol;
            var ySymbol = y?.MethodSymbol;
            if (SymbolEqualityComparer.Default.Equals(xSymbol, ySymbol))
                return true;
            if (xSymbol == null || ySymbol == null)
                return false;

            if (xSymbol.Name != ySymbol.Name)
                return false;
            var xParameters = xSymbol.Parameters;
            var yParameters = ySymbol.Parameters;
            if (xParameters.Length != yParameters.Length)
                return false;
            if (xSymbol.IsGenericMethod != ySymbol.IsGenericMethod)
                return false;
            var xGenericArgs = xSymbol.TypeArguments;
            var yGenericArgs = ySymbol.TypeArguments;
            if (xSymbol.IsGenericMethod && xGenericArgs.Length != yGenericArgs.Length)
                return false;
            for (int i = 0; i < xParameters.Length; i++)
            {
                var xParam = xParameters[i];
                var yParam = yParameters[i];
                if (xParam.Type.Kind != yParam.Type.Kind)
                {
                    return false;
                }
                if ((xParam.RefKind == RefKind.None) != (yParam.RefKind == RefKind.None))
                {
                    return false;
                }

                if (xParam.Type.Kind == SymbolKind.TypeParameter)
                {
                    if (xGenericArgs.IndexOf(xParam.Type) != yGenericArgs.IndexOf(yParam.Type))
                        return false;
                }
                else if (!SymbolEqualityComparer.Default.Equals(xParam.Type, yParam.Type))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(MethodModel model)
        {
            var obj = model?.MethodSymbol;
            if (obj == null)
                return 0;

            // We don't need everything, just be sensible
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.Name.GetHashCode();
                hash = hash * 23 + obj.Parameters.Length;
                return hash;
            }
        }
    }
}
