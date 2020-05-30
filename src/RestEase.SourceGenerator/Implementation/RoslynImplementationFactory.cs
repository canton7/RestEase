using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RestEase.Implementation;
using RestEase.Implementation.Emission;

namespace RestEase.SourceGenerator.Implementation
{
    public class RoslynImplementationFactory
    {
        private readonly DiagnosticReporter symbolsDiagnosticReporter;
        private readonly WellKnownSymbols wellKnownSymbols;
        private readonly AttributeInstantiator attributeInstantiator;
        private readonly Emitter emitter;
        private readonly HashSet<Diagnostic> symbolsDiagnostics = new HashSet<Diagnostic>();

        public RoslynImplementationFactory(Compilation compilation)
        {
            this.symbolsDiagnosticReporter = new DiagnosticReporter();
            this.wellKnownSymbols = new WellKnownSymbols(compilation, this.symbolsDiagnosticReporter);
            this.attributeInstantiator = new AttributeInstantiator(this.wellKnownSymbols);
            this.emitter = new Emitter(this.wellKnownSymbols);
        }

        public bool IsRestEaseAttribute(INamedTypeSymbol namedTypeSymbol) =>
            this.attributeInstantiator.IsRestEaseAttribute(namedTypeSymbol);

        public (SourceText? source, List<Diagnostic> diagnostics) CreateImplementation(INamedTypeSymbol namedTypeSymbol)
        {
            var diagnosticReporter = new DiagnosticReporter();
            var analyzer = new RoslynTypeAnalyzer(namedTypeSymbol, this.wellKnownSymbols, this.attributeInstantiator);
            var typeModel = analyzer.Analyze();
            var generator = new ImplementationGenerator(typeModel, this.emitter, diagnosticReporter);
            var emittedType = generator.Generate();

            // If there are symbols diagnostic errors, we have to fail this type.
            // However, we'll then clear the symbols diagnostics, so we can move onto the next type
            // (which might succeed).
            // We'll report the symbols diagnostics in one go at the end. We might well end up with duplicates
            // (WellKnownSymbols will keep reporting the same diagnostics), so use a HashSet.

            this.symbolsDiagnostics.UnionWith(this.symbolsDiagnosticReporter.Diagnostics);
            bool hasSymbolsErrors = this.symbolsDiagnosticReporter.HasErrors;
            this.symbolsDiagnosticReporter.Clear();

            return (hasSymbolsErrors || diagnosticReporter.HasErrors
                ? null
                : emittedType.SourceText, diagnosticReporter.Diagnostics);
        }

        public List<Diagnostic> GetCompilationDiagnostics() => this.symbolsDiagnostics.ToList();
    }
}
