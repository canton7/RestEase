using System;

namespace RestEase.Implementation.Analysis
{
    internal partial class AttributeModel<T> where T : Attribute
    {
        public override string AttributeName => this.Attribute.GetType().Name;

        public AttributeModel(T attribute)
        {
            this.Attribute = attribute;
        }
    }

    internal partial class AttributeModel
    {
        public static AttributeModel<T> Create<T>(T attribute) where T : Attribute => new AttributeModel<T>(attribute);
    }
}