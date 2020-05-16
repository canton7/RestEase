using System;

namespace RestEase.Implementation.Analysis
{
    internal abstract class AttributeModel
    {
        public abstract string AttributeName { get; }

        public static AttributeModel<T> Create<T>(T attribute) where T : Attribute => new AttributeModel<T>(attribute);
    }

    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public T Attribute { get; }
        public override string AttributeName => this.Attribute.GetType().Name;

        public AttributeModel(T attribute)
        {
            this.Attribute = attribute;
        }
    }
}