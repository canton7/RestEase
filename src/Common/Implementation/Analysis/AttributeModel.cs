using System;

namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        public abstract string AttributeName { get; }
    }

    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public T Attribute { get; }

        public override string AttributeName => this.Attribute.GetType().Name;
    }
}