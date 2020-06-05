using System;
using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        public AttributeData AttributeData { get; }

        protected AttributeModel(AttributeData attributeData)
        {
            this.AttributeData = attributeData;
        }

        public static AttributeModel<T> Create<T>(T attribute, AttributeData attributeData) where T : Attribute => new AttributeModel<T>(attribute, attributeData);
    }

    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public AttributeModel(T attribute, AttributeData attributeData)
            : base(attributeData)
        {
            this.Attribute = attribute;
        }
    }
}