using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class AllowAnyStatusCodeAttributeModel
    {
        public INamedTypeSymbol DeclaredOn { get; }

        public AllowAnyStatusCodeAttributeModel(AllowAnyStatusCodeAttribute attribute, INamedTypeSymbol namedTypeSymbol, AttributeData attributeData)
            : base(attribute, attributeData)
        {
            this.DeclaredOn = namedTypeSymbol;
        }

        public bool IsDeclaredOn(TypeModel typeModel) =>
            SymbolEqualityComparer.Default.Equals(this.DeclaredOn, typeModel.NamedTypeSymbol);
    }
}