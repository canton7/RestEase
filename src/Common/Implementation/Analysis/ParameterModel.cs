using System.Collections.Generic;

namespace RestEase.Implementation.Analysis
{
    internal partial class ParameterModel
    {
        public AttributeModel<HeaderAttribute>? HeaderAttribute { get; set; }
        public AttributeModel<PathAttribute>? PathAttribute { get; set; }
        public string? PathAttributeName => this.PathAttribute == null ? null : this.PathAttribute.Attribute.Name ?? this.Name;
        public AttributeModel<QueryAttribute>? QueryAttribute { get; set; }
        public string? QueryAttributeName => this.QueryAttribute == null ? null : (this.QueryAttribute.Attribute.HasName ? this.QueryAttribute.Attribute.Name : this.Name);
        public AttributeModel<HttpRequestMessagePropertyAttribute>? HttpRequestMessagePropertyAttribute { get; set; }
        public string? HttpRequestMessagePropertyAttributeKey => this.HttpRequestMessagePropertyAttribute == null ? null : this.HttpRequestMessagePropertyAttribute.Attribute.Key ?? this.Name;
        public AttributeModel<RawQueryStringAttribute>? RawQueryStringAttribute { get; set; }
        public AttributeModel<QueryMapAttribute>? QueryMapAttribute { get; set; }
        public AttributeModel<BodyAttribute>? BodyAttribute { get; set; }
        public bool IsCancellationToken { get; set; }

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
            if (this.RawQueryStringAttribute != null)
                yield return this.RawQueryStringAttribute;
            if (this.QueryMapAttribute != null)
                yield return this.QueryMapAttribute;
            if (this.BodyAttribute != null)
                yield return this.BodyAttribute;
        }
    }
}