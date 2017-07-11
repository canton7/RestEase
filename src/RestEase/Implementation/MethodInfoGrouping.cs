using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RestEase.Implementation
{
    internal class MethodInfoFieldReference
    {
        public int Id { get; }
        public MethodBuilder GeneratorMethod { get; }
        public FieldBuilder BackingField { get; }

        public MethodInfoFieldReference(int id, MethodBuilder generatorMethod, FieldBuilder backingField)
        {
            this.Id = id;
            this.GeneratorMethod = generatorMethod;
            this.BackingField = backingField;
        }
    }

    internal class MethodInfoGrouping
    {
        public List<MethodInfoFieldReference> Fields { get; } = new List<MethodInfoFieldReference>();
    }
}
