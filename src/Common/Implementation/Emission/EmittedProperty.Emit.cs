using System.Reflection.Emit;
using RestEase.Implementation.Analysis;

namespace RestEase.Implementation.Emission
{
    internal partial class EmittedProperty
    {
        public FieldBuilder FieldBuilder { get; }

        public EmittedProperty(PropertyModel propertyModel, FieldBuilder fieldBuilder)
        {
            this.PropertyModel = propertyModel;
            this.FieldBuilder = fieldBuilder;
        }
    }
}