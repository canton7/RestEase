using System;
using Microsoft.CodeAnalysis.Text;

namespace RestEase.Implementation.Emission
{
    internal class EmittedType
    {
        public SourceText SourceText { get; }

        public EmittedType(SourceText sourceText)
        {
            this.SourceText = sourceText;
        }
    }
}