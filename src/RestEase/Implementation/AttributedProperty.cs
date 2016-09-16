using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RestEase.Implementation
{
    internal interface IAttributedProperty
    {
        PropertyInfo PropertyInfo { get; }
        FieldInfo BackingField { get; set; }
    }

    internal class AttributedProperty<T> : IAttributedProperty
    {
        public T Attribute { get; }
        public PropertyInfo PropertyInfo { get; }
        public FieldInfo BackingField { get; set; }

        public AttributedProperty(T attribute, PropertyInfo propertyInfo)
        {
            this.Attribute = attribute;
            this.PropertyInfo = propertyInfo;
        }
    }
}
