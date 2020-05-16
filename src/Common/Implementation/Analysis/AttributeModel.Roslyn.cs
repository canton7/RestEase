using System;

namespace RestEase.Implementation.Analysis
{

    internal partial class AttributeModel
    {
        public static AttributeModel<T> Create<T>(T attribute) where T : Attribute => new AttributeModel<T>(attribute);
    }
}