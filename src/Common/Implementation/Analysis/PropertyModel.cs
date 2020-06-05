using System.Collections.Generic;

namespace RestEase.Implementation.Analysis
{
    internal partial class PropertyModel
    {
        public AttributeModel<HeaderAttribute>? HeaderAttribute { get; set; }
        public AttributeModel<PathAttribute>? PathAttribute { get; set; }
        public string? PathAttributeName => this.PathAttribute == null ? null : this.PathAttribute.Attribute.Name ?? this.Name;
        public AttributeModel<QueryAttribute>? QueryAttribute { get; set; }
        public string? QueryAttributeName => this.QueryAttribute == null ? null : this.QueryAttribute.Attribute.Name ?? this.Name;
        public AttributeModel<HttpRequestMessagePropertyAttribute>? HttpRequestMessagePropertyAttribute { get; set; }
        public string? HttpRequestMessagePropertyAttributeKey => this.HttpRequestMessagePropertyAttribute == null ? null : this.HttpRequestMessagePropertyAttribute.Attribute.Key ?? this.Name;
        public bool IsRequester { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }

        // Set by the ImplementationGenerator, not the TypeAnalyzer
        public bool IsExplicit { get; set; }

        public IEnumerable<AttributeModel> GetAllSetAttributes()
        {
            if (this.HeaderAttribute != null)
                yield return this.HeaderAttribute;
            if (this.PathAttribute != null)
                yield return this.PathAttribute;
            if (this.QueryAttribute != null)
                yield return this.QueryAttribute;
            if (this.HttpRequestMessagePropertyAttribute != null)
                yield return this.HttpRequestMessagePropertyAttribute;
        }
    }
}