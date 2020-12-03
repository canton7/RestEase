using System;
using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        public AttributeData AttributeData { get; }

        public ISymbol DeclaringSymbol { get; }

        protected AttributeModel(AttributeData attributeData, ISymbol declaringSymbol)
        {
            this.AttributeData = attributeData;
            this.DeclaringSymbol = declaringSymbol;
        }

        public static AttributeModel<T> Create<T>(T attribute, AttributeData attributeData, ISymbol declaringSymbol) where T : Attribute =>
            new(attribute, attributeData, declaringSymbol);

        public bool IsDeclaredOn(TypeModel typeModel) =>
            SymbolEqualityComparer.Default.Equals(this.DeclaringSymbol, typeModel.NamedTypeSymbol);
    }

    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public AttributeModel(T attribute, AttributeData attributeData, ISymbol declaringSymbol)
            : base(attributeData, declaringSymbol)
        {
            this.Attribute = attribute;
        }
    }
}