using System;
using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class PropertyModel
    {
        public IPropertySymbol PropertySymbol { get; }

        public string Name => this.PropertySymbol.Name;

        public bool IsNullable => throw new NotImplementedException();

        public PropertyModel(IPropertySymbol propertySymbol)
        {
            this.PropertySymbol = propertySymbol;
        }
    }
}