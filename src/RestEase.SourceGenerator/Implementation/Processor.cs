using Microsoft.CodeAnalysis;

namespace RestEase.SourceGenerator.Implementation
{
    internal class Processor : SymbolVisitor
    {
        private readonly SourceGeneratorContext context;
        private readonly RoslynImplementationFactory factory;

        public Processor(SourceGeneratorContext context)
        {
            this.context = context;
            this.factory = new RoslynImplementationFactory(context.Compilation);
        }

        public void Process()
        {
            try
            {
                this.context.Compilation.GlobalNamespace.Accept(this);
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

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeKind == TypeKind.Interface)
            {
                if (this.ShouldProcessType(namedTypeSymbol))
                {
                    this.ProcessType(namedTypeSymbol);
                }
            }
            else if (namedTypeSymbol.TypeKind == TypeKind.Class)
            {
                // Handle nested types
                foreach (var member in namedTypeSymbol.GetMembers())
                {
                    if (member.Kind == SymbolKind.NamedType)
                    {
                        member.Accept(this);
                    }
                }
            }
        }

        private bool ShouldProcessType(INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var memberSymbol in namedTypeSymbol.GetMembers())
            {
                foreach (var attributeData in memberSymbol.GetAttributes())
                {
                    if (attributeData.AttributeClass != null &&
                        this.factory.IsRestEaseAttribute(attributeData.AttributeClass))
                    {
                        return true;
                    }
                }
            }

            return false;
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
                this.context.AddSource("RestEase_" + namedTypeSymbol.Name, sourceText);
            }
        }
    }
}
