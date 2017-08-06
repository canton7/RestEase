using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RestEase.Implementation
{
    internal class MethodInfoFieldReference
    {
        public MethodInfo MethodInfo { get; }
        public FieldBuilder BackingField { get; }

        public MethodInfoFieldReference(MethodInfo methodInfo, FieldBuilder backingField)
        {
            this.MethodInfo = methodInfo;
            this.BackingField = backingField;
        }
    }

    internal class MethodInfoGrouping
    {
        public List<MethodInfoFieldReference> Fields { get; } = new List<MethodInfoFieldReference>();
    }
}
