using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RestEase.Implementation.Analysis
{
    internal partial class TypeModel
    {
        public List<AttributeModel<HeaderAttribute>> HeaderAttributes { get; } = new List<AttributeModel<HeaderAttribute>>();
        public List<AllowAnyStatusCodeAttributeModel> AllowAnyStatusCodeAttributes { get; } = new List<AllowAnyStatusCodeAttributeModel>();
        public AllowAnyStatusCodeAttributeModel? TypeAllowAnyStatusCodeAttribute => this.AllowAnyStatusCodeAttributes.FirstOrDefault(x => x.ContainingType == this.Type);
        public AttributeModel<SerializationMethodsAttribute>? SerializationMethodsAttribute { get; set; }
        public AttributeModel<BasePathAttribute>? BasePathAttribute { get; set; }
        public List<EventModel> Events { get; } = new List<EventModel>();
        public List<PropertyModel> Properties { get; } = new List<PropertyModel>();
        public List<MethodModel> Methods { get; } = new List<MethodModel>();
    }
}