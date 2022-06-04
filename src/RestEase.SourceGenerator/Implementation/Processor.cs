using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RestEase.SourceGenerator.Implementation
{
    internal class Processor
    {
        private readonly GeneratorExecutionContext context;
        private readonly RoslynImplementationFactory factory;

        public Processor(GeneratorExecutionContext context)
        {
            this.context = context;
            this.factory = new RoslynImplementationFactory(context.Compilation);
        }

        public void Process()
        {
            if (this.context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
                return;

            try
            {
                this.ProcessMemberSyntaxes(syntaxReceiver.MemberSyntaxes);
            }
            finally // Just in case we crash...
            {
                // Report the compilation-level diagnostics
                foreach (var diagnostic in this.factory.GetCompilationDiagnostics())
                {
                    this.context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void ProcessMemberSyntaxes(IEnumerable<MemberDeclarationSyntax> memberSyntaxes)
        {
            var visitedTypes = new HashSet<INamedTypeSymbol>();
            foreach (var memberSyntax in memberSyntaxes)
            {
                var memberSymbol = this.context.Compilation
                    .GetSemanticModel(memberSyntax.SyntaxTree)
                    .GetDeclaredSymbol(memberSyntax);
                var containingType = memberSymbol?.ContainingType;
                if (containingType != null
                    && containingType.TypeKind == TypeKind.Interface
                    && visitedTypes.Add(containingType))
                {
                    foreach (var attributeData in memberSymbol!.GetAttributes())
                    {
                        if (attributeData.AttributeClass != null &&
                            this.factory.IsRestEaseAttribute(attributeData.AttributeClass))
                        {
                            this.ProcessType(containingType);
                            break;
                        }
                    }
                }

                this.context.CancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void ProcessType(INamedTypeSymbol namedTypeSymbol)
        {
            var (sourceText, diagnostics) = this.factory.CreateImplementation(namedTypeSymbol);
            foreach (var diagnostic in diagnostics)
            {
                this.context.ReportDiagnostic(diagnostic);
            }

            if (sourceText != null)
            {
                this.context.AddSource("RestEase_" + namedTypeSymbol.Name + ".g", sourceText);
            }
        }
    }
}
