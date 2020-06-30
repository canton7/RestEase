using System;

namespace RestEase.Implementation.Emission
{
    internal class EmittedType
    {
        public Type Type { get; }

        public EmittedType(Type type) => this.Type = type;
    }
}