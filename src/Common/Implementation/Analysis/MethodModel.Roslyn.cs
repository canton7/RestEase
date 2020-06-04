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

        public bool IsDeclaredOn(TypeModel typeModel) =>
            SymbolEqualityComparer.Default.Equals(typeModel.NamedTypeSymbol, this.MethodSymbol.ContainingType);
    }
}