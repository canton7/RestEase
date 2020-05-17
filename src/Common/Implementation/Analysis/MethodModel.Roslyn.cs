using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class MethodModel
    {
        public IMethodSymbol MethodSymbol { get; }

        public MethodModel(IMethodSymbol methodSymbol)
        {
            this.MethodSymbol = methodSymbol;
        }
    }
}