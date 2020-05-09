namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        public abstract string AttributeName { get; }
    }

    internal partial class AttributeModel<T> : AttributeModel
    {
        public T Attribute { get; }
    }
}