
using System;

namespace RestEase.Implementation.Analysis
{
    internal partial class TypeModel
    {
        public Type Type { get; }

        public TypeModel(Type type)
        {
            this.Type = type;
        }
    }
}