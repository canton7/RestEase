using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class TypeModel
    {
        public INamedTypeSymbol NamedTypeSymbol { get; }

        public TypeModel(INamedTypeSymbol namedTypeSymbol)
        {
            this.NamedTypeSymbol = namedTypeSymbol;
        }
    }
}