using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class AllowAnyStatusCodeAttributeModel
    {
        public INamedTypeSymbol DefinedOn { get; }

        public AllowAnyStatusCodeAttributeModel(AllowAnyStatusCodeAttribute attribute, INamedTypeSymbol namedTypeSymbol, AttributeData attributeData)
            : base(attribute, attributeData)
        {
            this.DefinedOn = namedTypeSymbol;
        }

        public bool IsDefinedOn(TypeModel typeModel) => SymbolEqualityComparer.Default.Equals(this.DefinedOn, typeModel.NamedTypeSymbol);
    }
}