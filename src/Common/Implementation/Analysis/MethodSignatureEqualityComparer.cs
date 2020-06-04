using System.Collections.Generic;

namespace RestEase.Implementation.Analysis
{
    internal partial class MethodSignatureEqualityComparer : IEqualityComparer<MethodModel>
    {
        public static MethodSignatureEqualityComparer Instance { get; } = new MethodSignatureEqualityComparer();
    }
}
