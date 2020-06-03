using System.Collections.Generic;

namespace RestEase.Implementation.Analysis
{
    internal partial class MethodModel
    {
        public AttributeModel<RequestAttributeBase>? RequestAttribute { get; set; }
        public AttributeModel<AllowAnyStatusCodeAttribute>? AllowAnyStatusCodeAttribute { get; set; }
        public AttributeModel<SerializationMethodsAttribute>? SerializationMethodsAttribute { get; set; }
        public List<AttributeModel<HeaderAttribute>> HeaderAttributes { get; } = new List<AttributeModel<HeaderAttribute>>();
        public bool IsDisposeMethod { get; set; }

        public List<ParameterModel> Parameters { get; } = new List<ParameterModel>();
    }
}