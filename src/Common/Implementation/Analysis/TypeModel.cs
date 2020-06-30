using System.Collections.Generic;
using System.Linq;

namespace RestEase.Implementation.Analysis
{
    internal partial class TypeModel
    {
        public List<AttributeModel<HeaderAttribute>> HeaderAttributes { get; } = new List<AttributeModel<HeaderAttribute>>();
        public List<AttributeModel<AllowAnyStatusCodeAttribute>> AllowAnyStatusCodeAttributes { get; } = new List<AttributeModel<AllowAnyStatusCodeAttribute>>();
        public AttributeModel<AllowAnyStatusCodeAttribute>? TypeAllowAnyStatusCodeAttribute => this.AllowAnyStatusCodeAttributes.FirstOrDefault(x => x.IsDeclaredOn(this));
        public AttributeModel<SerializationMethodsAttribute>? SerializationMethodsAttribute { get; set; }
        public AttributeModel<BasePathAttribute>? BasePathAttribute { get; set; }
        public bool IsAccessible { get; set; }
        public List<EventModel> Events { get; } = new List<EventModel>();
        public List<PropertyModel> Properties { get; } = new List<PropertyModel>();
        public List<MethodModel> Methods { get; } = new List<MethodModel>();
    }
}