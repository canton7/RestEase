using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class PropertyModel
    {
        public IPropertySymbol PropertySymbol { get; }

        public string Name => this.PropertySymbol.Name;

        public bool IsNullable => this.PropertySymbol.Type.IsReferenceType ||
            this.PropertySymbol.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

        public PropertyModel(IPropertySymbol propertySymbol)
        {
            this.PropertySymbol = propertySymbol;
        }

        public bool IsDeclaredOn(TypeModel typeModel) =>
            SymbolEqualityComparer.Default.Equals(typeModel.NamedTypeSymbol, this.PropertySymbol.ContainingType);
    }
}