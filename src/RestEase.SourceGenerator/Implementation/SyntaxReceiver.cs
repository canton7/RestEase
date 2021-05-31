using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RestEase.SourceGenerator.Implementation
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MemberDeclarationSyntax> MemberSyntaxes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Actually matching the attributes is hard -- they don't have to be qualified, and we've
            // lot loads of attribute types. Just pick up on all members which have attributes, and we'll
            // filter them properly in Processor.

            if (syntaxNode is MemberDeclarationSyntax member &&
                member.SyntaxTree != null &&
                (member is PropertyDeclarationSyntax || member is MethodDeclarationSyntax) &&
                member.AttributeLists.Count > 0)
            {
                this.MemberSyntaxes.Add(member);
            }
        }
    }
}
