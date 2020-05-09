using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RestEase.SourceGenerator
{
    internal class TypeGenerator
    {
        private readonly StringBuilder sb = new StringBuilder();

        public void AddTypeDeclaration(INamedTypeSymbol parent)
        {
        }

        public override string ToString() => this.sb.ToString();
    }
}
