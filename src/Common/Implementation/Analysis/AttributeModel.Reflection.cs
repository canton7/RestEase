using System;

namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        public static AttributeModel<T> Create<T>(T attribute) where T : Attribute => new AttributeModel<T>(attribute);
    }


    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public AttributeModel(T attribute)
        {
            this.Attribute = attribute;
        }
    }
}